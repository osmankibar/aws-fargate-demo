using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace MainServer
{
    public class Manager
    {
        private readonly ConcurrentDictionary<Guid, Agent> _agents;
        private readonly IHttpClientFactory _clientFactory;

        public Manager(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _agents = new ConcurrentDictionary<Guid, Agent>();
        }

        public Agent[] GetAgent()
        {
            return _agents.Values.ToArray();
        }

        public void Update(Guid id, string status)
        {
            var agent = _agents[id];

            if (agent == null) return;

            lock (agent)
            {
                switch (status)
                {
                    case "Finished":
                        agent.Status = Status.Finished;
                        agent.FinishedDate = DateTime.UtcNow;
                        break;
                    case "Running":
                        if (agent.Status != Status.Finished)
                        {
                            agent.Status = Status.Running;
                            agent.RunningDate = DateTime.UtcNow;
                        }
                        break;
                    default:
                        agent.Status = agent.Status;
                        break;
                }
            }
        }

        public async Task DeployPodAsync(string serverType, int totalPod)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Pod.tmpl");

            var template = Handlebars.Compile(await File.ReadAllTextAsync(path));

            for (var i = 0; i < totalPod; i++)
            {
                var agent = new Agent()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ServerType = serverType == "Fargate" ? ServerType.Fargate : ServerType.CA,
                    Status = Status.Pending,
                    RequestDate = DateTime.UtcNow
                };
                var @namespace = agent.ServerType == ServerType.Fargate ? "agent" : "worker";
                var templateData = new
                {
                    AgentId = agent.Id,
                    ServerType = agent.ServerType.ToString().ToLower(),
                    Image = Environment.GetEnvironmentVariable("ASPNETCORE_AGENT_IMAGE"),
                    Host = Environment.GetEnvironmentVariable("ASPNETCORE_HOST"),
                    Namespace = @namespace,
                };

                var renderedFileContent = template(templateData);

                var client = _clientFactory.CreateClient("clientAgent");

                var content = new StringContent(renderedFileContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"/api/v1/namespaces/{@namespace}/pods", content);

                if (response.IsSuccessStatusCode)
                {
                    _agents.TryAdd(new Guid(agent.Id), agent);
                }
                else
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }

        public async Task ClearAsync()
        {
            var client = _clientFactory.CreateClient("clientAgent");

            foreach (var (key, agent) in _agents)
            {
                var @namespace = agent.ServerType == ServerType.Fargate ? "agent" : "worker";
                var response = await client.DeleteAsync($"/api/v1/namespaces/{@namespace}/pods/{agent.Id}-{agent.ServerType.ToString().ToLower()}");

                if (response.IsSuccessStatusCode)
                    _agents.TryRemove(key, out _);
            }
        }
    }

    public class Agent
    {
        public string Id { get; set; }
        public ServerType ServerType { get; set; }
        public Status Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime RunningDate { get; set; }
        public DateTime FinishedDate { get; set; }
    }

    public enum ServerType
    {
        Fargate,
        CA, // cluster auto scaler
    }

    public enum Status
    {
        Pending,
        Running,
        Finished,
    }
}