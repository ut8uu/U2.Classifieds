using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;

namespace U2.Classifieds.Core;

public abstract class ProcessorBase
{
    private readonly ClassifiedsService _service;
    private readonly Timer _topicLoadTimer;
    private readonly Timer _branchLoadTimer;
    private readonly Timer _imageLoadTimer;
    private readonly HttpClient _client = new();
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly int _numberOfRetriesOnFailure = 5;
    private readonly TimeSpan _delayBeforeRetryOnFailure = TimeSpan.FromSeconds(30);
    private readonly Encoding _win1251;
    private CancellationTokenSource _cancellationTokenSource = new();

    protected ProcessorBase(SvcSettings svcSettings)
    {
        SvcSettings = svcSettings;
        var db = CreateDatabase();
        var storage = new ClassifiedsStorage(db);
        _service = new ClassifiedsService(storage);

        _topicLoadTimer = new Timer(LoadTopicPageCallbackFunc);
        StopLoadTopicTimer();
        _branchLoadTimer = new Timer(LoadBranchPageCallbackFunc);
        StopLoadBranchTimer();
        _imageLoadTimer = new Timer(LoadImageCallbackFunc);
        StopLoadImageTimer();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _win1251 = Encoding.GetEncoding("windows-1251");

        _retryPolicy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: _numberOfRetriesOnFailure,
                sleepDurationProvider: delay => _delayBeforeRetryOnFailure);

    }

    private async Task<HttpResponseMessage> WrapRequestAsync(
        Func<Task<HttpResponseMessage>> requestFunction)
    {
        return await _retryPolicy.ExecuteAsync(async () => await requestFunction());
    }

    private async Task<T> WrapRequestAsync<T>(
        Func<Task<HttpResponseMessage>> requestFunction,
        string url)
    {
        using var response = await WrapRequestAsync(requestFunction);
        var content = await response.Content.ReadAsStringAsync(Token);
        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(content, typeof(T));
        }
        return  JsonConvert.DeserializeObject<T>(content);
    }
    
    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartLoadBranchTimer();
        StartLoadTopicTimer();
        StartLoadImageTimer();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        StopLoadBranchTimer();
        StopLoadTopicTimer();
        StopLoadImageTimer();
    }

    private void StopLoadTopicTimer()
    {
        _topicLoadTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void StartLoadTopicTimer()
    {
        _topicLoadTimer.Change(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
    }

    private void LoadTopicPageCallbackFunc(object state)
    {
        lock (LoadTopicPageLockObject)
        {
            StopLoadTopicTimer();
            try
            {
                var task = LoadTopicPageAsync();
                task.Wait(Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                StartLoadTopicTimer();
            }
        }
    }

    protected virtual Task LoadTopicPageAsync()
    {
        throw new NotImplementedException();
    }

    public object LoadTopicPageLockObject => new object();

    #region Branches

    private void StopLoadBranchTimer()
    {
        _branchLoadTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void StartLoadBranchTimer()
    {
        _branchLoadTimer.Change(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
    }

    private void LoadBranchPageCallbackFunc(object state)
    {
        lock (LoadBranchPageLockObject)
        {
            StopLoadBranchTimer();
            try
            {
                var task = LoadBranchPageAsync();
                task.Wait(Token);
            }
            finally
            {
                StartLoadBranchTimer();
            }
        }
    }

    protected virtual Task LoadBranchPageAsync()
    {
        throw new NotImplementedException();
    }

    public object LoadBranchPageLockObject => new object();

    #endregion

    #region Images

    private void StopLoadImageTimer()
    {
        _imageLoadTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void StartLoadImageTimer()
    {
        _imageLoadTimer.Change(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
    }

    private void LoadImageCallbackFunc(object state)
    {
        lock (LoadImageLockObject)
        {
            StopLoadImageTimer();
            try
            {
                var task = LoadImageAsync();
                task.Wait(Token);
            }
            finally
            {
                StartLoadImageTimer();
            }
        }
    }

    protected virtual Task LoadImageAsync()
    {
        throw new NotImplementedException();
    }

    public object LoadImageLockObject => new();

    #endregion

    public SvcSettings SvcSettings { get; }

    public ClassifiedsService Service => _service;

    public CancellationToken Token => _cancellationTokenSource.Token;

    protected IMongoDatabase CreateDatabase()
    {
        var client = new MongoClient(SvcSettings.ConnectionString);
        return client.GetDatabase(SvcSettings.DatabaseName);
    }

    async Task<string> DownloadUrlAsync(string url, CancellationToken cancellationToken)
    {
        return await WrapRequestAsync<string>(async () => await _client.GetAsync(url, cancellationToken), url);
    }

    protected async Task<string> DownloadAsync(UrlInfo url, UrlKind urlKind, CancellationToken cancellationToken)
    {
        var cache = urlKind switch
        {
            UrlKind.Branch => FileCache.TryGetBranchCache(url.Url),
            UrlKind.Topic => FileCache.TryGetTopicCache(url.Url),
            _ => ""
        };
        if (!string.IsNullOrEmpty(cache))
        {
            url.UrlLoadState = UrlLoadState.Loaded;
            url.UrlLoadStatusCode = UrlLoadStatusCode.Success;
            return cache;
        }

        var responseString = await DownloadUrlAsync(url.Url, cancellationToken);
        url.UrlLoadState = UrlLoadState.Loaded;
        url.UrlLoadStatusCode = UrlLoadStatusCode.Success;

        if (responseString == null)
        {
            return null;
        }
        switch (urlKind)
        {
            case UrlKind.Branch:
                FileCache.PutBranchCache(url.Url, responseString);
                break;
            case UrlKind.Topic:
                FileCache.PutTopicCache(url.Url, responseString);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(urlKind), urlKind, null);
        }

        return responseString;
    }

    protected async Task<bool> DownloadImageAsync(UrlInfo url, CancellationToken cancellationToken)
    {
        var imageFileName = FileCache.UrlToPath("Images", url.Url, "tiff");
        if (File.Exists(imageFileName))
        {
            return true;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(imageFileName));
            var stream = await _client.GetStreamAsync(url.Url, cancellationToken);
            var destStream = File.OpenWrite(imageFileName);
            await stream.CopyToAsync(destStream, cancellationToken);
            destStream.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;

    }
}

public enum UrlKind
{
    Branch,
    Topic,
    Image,
}
