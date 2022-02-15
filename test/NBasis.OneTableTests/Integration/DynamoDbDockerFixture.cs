using Amazon.DynamoDBv2;
using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NBasis.OneTableTests.Integration
{
    public class DynamoDbDockerFixture : IDisposable
    {
        readonly DockerClient _docker;
        private string _containerId;
        private int _port;
        private IAmazonDynamoDB _client;

        public DynamoDbDockerFixture()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _docker = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            else
                _docker = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

            StartContainer().GetAwaiter().GetResult();
        }

        public IAmazonDynamoDB DynamoDbClient
        {
            get
            {
                if (_client == null)
                {
                    _client = new AmazonDynamoDBClient(
                        new Amazon.Runtime.BasicAWSCredentials("key", "secret"), 
                        new AmazonDynamoDBConfig
                        {
                            ServiceURL = $"http://{IPAddress.Loopback}:{_port}"
                        });
                }
                return _client;
            }
        }

        public async Task StartContainer()
        {
            await _docker.Images.CreateImageAsync(new ImagesCreateParameters() 
                { 
                    FromImage = "amazon/dynamodb-local", Tag = "latest" 
                }, new AuthConfig(), new Progress<JSONMessage>());

            // get an open port
            TcpListener l = new(IPAddress.Loopback, 0);
            l.Start();
            _port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();

            var hostConfig = new HostConfig()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { $"8000/tcp", new List<PortBinding> 
                        { 
                            new PortBinding 
                            { 
                                HostIP = IPAddress.Loopback.ToString(), 
                                HostPort = _port.ToString() 
                            } 
                        } 
                    }
                }
            };

            var response = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters()
                {
                    Hostname = "localhost",
                    Image = "amazon/dynamodb-local:latest",
                    Name = string.Format("onetable-fixture-{0:N}", Guid.NewGuid()),
                    Tty = false,
                    HostConfig = hostConfig,
                });

            _containerId = response.ID;

            await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters()).Wait();
                    _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters()).Wait();
                    _docker.Dispose();
                }

                disposed = true;
            }
        }
    }
}
