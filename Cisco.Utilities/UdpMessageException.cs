using System;
using System.Runtime.Serialization;

namespace Cisco.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    public class UdpMessageException : Exception
    {
        /// <summary>
        /// Gets or sets the UDP message.
        /// </summary>
        /// <value>
        /// The UDP message.
        /// </value>
        public string UdpMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpMessageException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="udpMessage">The UDP message.</param>
        public UdpMessageException(string message, string udpMessage) : base(message)
        {
            this.UdpMessage = udpMessage;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpMessageException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UdpMessageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpMessageException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public UdpMessageException(string message, Exception inner) : base(message, inner)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpMessageException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="udpMessage">The UDP message.</param>
        /// <param name="inner">The inner.</param>
        public UdpMessageException(string message, string udpMessage, Exception inner) : base(message, inner)
        {
            this.UdpMessage = udpMessage;
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

}
