using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class LogPageSampleViewModel : ViewModelBase
    {
        private readonly ILogService _logStore;


        private readonly Random _rand = new();

        private readonly ObservableFixedSizeRingBuffer<string> _buffer = new(10);
        private readonly ISynchronizedView<string, string> _synchronizedView;

        private readonly ObservableDictionary<int, string> _dictionary = new();
        private readonly ISynchronizedView<KeyValuePair<int, string>, string> _synchronizedDictView;

        public NotifyCollectionChangedSynchronizedViewList<string> Logs { get; }

        public BindableReactiveProperty<string> InsertionText { get; } = new();
        public BindableReactiveProperty<string> FilterText { get; } = new();

        public ReactiveCommand LogDebugCommand { get; }
        public ReactiveCommand LogInfoCommand { get; }
        public ReactiveCommand LogDebugParalellCommand { get; }


        public LogPageSampleViewModel(ILogService logStore)
        {
            _logStore = logStore;
            LogDebugCommand = new ReactiveCommand(_ => _logStore.LogDebug("debug")).AddTo(Disposables);
            LogInfoCommand = new ReactiveCommand(_ => _logStore.LogInfo("info")).AddTo(Disposables);
            LogDebugParalellCommand = new ReactiveCommand(_ =>
            {
                //var subject = new Subject<int>();

                //subject
                //    .Select(id => $"thread {id}")
                //    .SubscribeAwait(async (s, ct) =>
                //    {
                //        await Task.Run(() => _logService.LogDebug(s), ct);
                //    }, AwaitOperation.Parallel, maxConcurrent: 2);

                //for (int i = 0; i < 30; ++i)
                //{
                //    subject.OnNext(i);
                //}

                // メインスレッドからのログ
                _logStore.LogDebug("Log from main thread");

                // 異なるスレッドからログを記録
                Task[] tasks = new Task[5];
                for (int i = 0; i < tasks.Length; i++)
                {
                    int taskId = i; // ローカルコピーを作成してクロージャを回避
                    tasks[i] = Task.Run(() =>
                    {
                        _logStore.LogDebug($"Log from task {taskId}");
                    });
                }

                // 全てのタスクが完了するまで待機
                Task.WaitAll(tasks);

            }).AddTo(Disposables);

            var weekdays = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri" };
            foreach (var week in weekdays)
            {
                _buffer.AddLast(week);
            }
            foreach (var (wee, id) in weekdays.Zip(Enumerable.Range(0, 10)))
            {
                _dictionary.Add(id, wee);
            }

            _synchronizedView = _buffer.CreateView(s =>
            {
                System.Diagnostics.Debug.WriteLine(s);
                return s;
            });
            Logs = _synchronizedView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            _synchronizedDictView = _dictionary.CreateView(item => $"id: {item.Key}, week: {item.Value}");
            //Logs = _synchronizedDictView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

            FilterText
                .Skip(1)
                .Debounce(TimeSpan.FromMilliseconds(300))
                .Subscribe(_ => RefleshView())
                .AddTo(Disposables);

            AddLastCommand = InsertionText
                .Select(s => !string.IsNullOrEmpty(s))
                .ToReactiveCommand<bool>(clear =>
                {
                    _buffer.AddLast(InsertionText.Value);

                    int id = _rand.Next(0, 10);
                    if (_dictionary.ContainsKey(id))
                    {
                        System.Diagnostics.Debug.WriteLine($"same id: {id}");
                    }
                    _dictionary[id] = InsertionText.Value;


                    if (clear)
                    {
                        InsertionText.Value = string.Empty;
                    }
                }).AddTo(Disposables);

            AddFirstCommand = InsertionText
                .Select(s => !string.IsNullOrEmpty(s))
                .ToReactiveCommand(_ =>
                {
                    _buffer.AddFirst(InsertionText.Value);
                }).AddTo(Disposables);

            PopLastCommand = _buffer
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToReactiveCommand(_ =>
                {
                    _buffer.RemoveLast();
                }).AddTo(Disposables);

            PopFirstCommand = _buffer
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToReactiveCommand(_ =>
                {
                    _buffer.RemoveFirst();
                }).AddTo(Disposables);
        }

        private void RefleshView()
        {
            _synchronizedView.AttachFilter((string search) =>
            {
                if (!string.IsNullOrEmpty(FilterText.Value) && !search.Contains(FilterText.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
                return true;
            });
        }

        public ReactiveCommand<bool> AddLastCommand { get; }
        public ReactiveCommand AddFirstCommand { get; }
        public ReactiveCommand PopLastCommand { get; }
        public ReactiveCommand PopFirstCommand { get; }
    }


    class TestFilter : ISynchronizedViewFilter<string, string>
    {
        public bool IsMatch(string value, string view)
        {
            throw new NotImplementedException();
        }

    }
}
