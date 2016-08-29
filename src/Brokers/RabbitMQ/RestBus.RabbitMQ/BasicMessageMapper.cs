using RestBus.Client;
using RestBus.Common.Amqp;
using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;

namespace RestBus.RabbitMQ
{
    public class BasicMessageMapper : IMessageMapper
    {
        protected readonly string[] amqpHostUris;
        protected readonly string serviceName;

        public BasicMessageMapper(string amqpHostUri, string serviceName)
        {
            if(String.IsNullOrWhiteSpace(amqpHostUri))
            {
                throw new ArgumentException("amqpHostUri");
            }

            if (String.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("serviceName");
            }

            this.amqpHostUris = new string[] { amqpHostUri };
            this.serviceName = serviceName;

            ServerUris = amqpHostUris.Select(u => new AmqpConnectionInfo { Uri = u, FriendlyName = StripUserInfoAndQuery(u) }).ToArray();
            SupportedExchangeKinds = ExchangeKind.Direct;
        }

        public virtual IList<AmqpConnectionInfo> ServerUris { get; protected set; }
        public virtual ExchangeKind SupportedExchangeKinds { get; protected set; }

        public virtual MessagingConfiguration MessagingConfig
        {
            get
            {
                return new MessagingConfiguration();
            }
        }

        public virtual string GetServiceName(HttpRequestMessage request)
        {
            //TODO: Have a static helper that callers to GetServiceName will use to check that for valid servicenames.

            return serviceName;
        }

        public virtual string GetRoutingKey(HttpRequestMessage request, ExchangeKind exchangeKind)
        {
            return null;
        }

        /// <summary>
        /// Gets the Headers for the message.
        /// </summary>
        /// <remarks>
        /// This is only useful for the headers exchange type.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual IDictionary<string, object> GetHeaders(HttpRequestMessage request)
        {
            return null;
        }

        /// <summary>
        /// Returns the RequestOptions associated with a specified request.
        /// </summary>
        /// <remarks>
        /// This helper is useful for classes deriving from BasicMessageMapper.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RequestOptions GetRequestOptions(HttpRequestMessage request)
        {
            return MessageInvokerBase.GetRequestOptions(request);
        }

        /// <summary>
        ///  Removes the username, password and query components of an AMQP uri.
        /// </summary>
        public static string StripUserInfoAndQuery(string amqpUri)
        {
            if(amqpUri == null)
            {
                throw new ArgumentNullException("amqpUri");
            }

            amqpUri = amqpUri.Trim();

            int startIndex;
#if NETCORE
            if (amqpUri.Length > 8 && amqpUri.StartsWith("amqps://", StringComparison.CurrentCultureIgnoreCase))
#else
            if(amqpUri.Length > 8 && amqpUri.StartsWith("amqps://", StringComparison.InvariantCultureIgnoreCase))
#endif
            {
                startIndex = 8;
            }
#if NETCORE
            else if (amqpUri.Length > 7 && amqpUri.StartsWith("amqp://", StringComparison.CurrentCultureIgnoreCase))
#else
            else if (amqpUri.Length > 7 && amqpUri.StartsWith("amqp://", StringComparison.InvariantCultureIgnoreCase))
#endif
            {
                startIndex = 7;
            }
            else
            {
                throw new ArgumentException("amqpUri is not in expected format.");
            }

            int endIndex = amqpUri.IndexOf('@');
            if (endIndex >= 0)
            {
                amqpUri = amqpUri.Remove(startIndex, (endIndex - startIndex) + 1);
            }

            int queryIndex = amqpUri.IndexOf('?');
            if (queryIndex >= 0)
            {
                amqpUri = amqpUri.Substring(0, queryIndex);
            }

            return amqpUri;
        }
    }

}
