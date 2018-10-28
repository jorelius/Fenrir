using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;

namespace Fenrir.Core
{
    public class Agent
    {
        private readonly IAgentJob _AgentJob;

        public Agent(IAgentJob AgentJob)
        {
            _AgentJob = AgentJob;
        }

        #region Load Test

        public Task<AgentResult> Run(int threads, TimeSpan duration, CancellationToken cancellationToken)
        {
            return Run(threads, duration, null, cancellationToken);
        }

        public Task<AgentResult> Run(int count, CancellationToken cancellationToken)
        {
            return Run(1, TimeSpan.MaxValue, count, cancellationToken);
        }

        private Task<AgentResult> Run(int threads, TimeSpan duration, int? count, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var combinedAgentThreadResult = QueueAgentThreads(threads, duration, count, cancellationToken);
                var AgentResult = new AgentResult(threads, combinedAgentThreadResult.Elapsed);
                AgentResult.Process(combinedAgentThreadResult);
                return AgentResult;
            });
        }

        private CombinedAgentThreadResult QueueAgentThreads(int threads, TimeSpan duration, int? count, CancellationToken cancellationToken)
        {
            var results = new ConcurrentQueue<AgentThreadResult>();
            var events = new List<ManualResetEventSlim>();
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < threads; i++)
            {
                var resetEvent = new ManualResetEventSlim(false);

                Thread thread;

                if (count.HasValue)
                    thread = new Thread(async (index) => await DoWork_Count(count.Value, results, cancellationToken, resetEvent, (int)index));
                else
                    thread = new Thread(async (index) => await DoWork_Duration(duration, sw, results, cancellationToken, resetEvent, (int)index));
                
                thread.Start(i);
                events.Add(resetEvent);
            }

            for (var i = 0; i < events.Count; i += 50)
            {
                var group = events.Skip(i).Take(50).Select(r => r.WaitHandle).ToArray();
                WaitHandle.WaitAll(group);
            }

            return new CombinedAgentThreadResult(results, sw.Elapsed);
        }

        private async Task DoWork_Duration(TimeSpan duration, Stopwatch sw, ConcurrentQueue<AgentThreadResult> results, CancellationToken cancellationToken, ManualResetEventSlim resetEvent, int AgentIndex)
        {
            IAgentJob job;
            var AgentThreadResult = new AgentThreadResult();

            try
            {
                job = await _AgentJob.Init(AgentIndex, AgentThreadResult);
            }
            catch (Exception)
            {
                AgentThreadResult.AddError((int)sw.ElapsedMilliseconds, 0, false);
                results.Enqueue(AgentThreadResult);
                resetEvent.Set();
                return;
            }

            while (!cancellationToken.IsCancellationRequested && duration.TotalMilliseconds > sw.Elapsed.TotalMilliseconds)
            {
                try
                {
                    await job.DoWork();
                }
                catch (Exception)
                {
                    AgentThreadResult.AddError((int)sw.ElapsedMilliseconds, 0, false);
                }
            }
            
            results.Enqueue(job.GetResults());
            resetEvent.Set();
        }

        private async Task DoWork_Count(int count, ConcurrentQueue<AgentThreadResult> results, CancellationToken cancellationToken, ManualResetEventSlim resetEvent, int AgentIndex)
        {
            var AgentThreadResult = new AgentThreadResult();
            var job = await _AgentJob.Init(AgentIndex, AgentThreadResult);

            for (var i = 0; i < count && !cancellationToken.IsCancellationRequested; i++)
            {
                await job.DoWork();
            }

            results.Enqueue(job.GetResults());
            resetEvent.Set();
        }

        #endregion Load Test

        public Task<AgentResult> Run(int threads, JsonHttpRequestTree requestTree, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return Run(threads, duration, null, cancellationToken);
        }
    }
}