////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>The exception that is thrown when a <see cref="BitstampClient"/> method fails.</summary>
    [Serializable]
    public sealed class BitstampException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="BitstampException"/> class.</summary>
        public BitstampException()
            : base()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BitstampException"/> class.</summary>
        public BitstampException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BitstampException"/> class.</summary>
        public BitstampException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private BitstampException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
