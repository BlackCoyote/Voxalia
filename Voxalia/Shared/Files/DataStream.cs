//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.IO;

namespace Voxalia.Shared.Files
{
    /// <summary>
    /// Wraps a System.IO.MemoryStream.
    /// </summary>
    public class DataStream : MemoryStream
    {
        /// <summary>
        /// Constructs a data stream with bytes pre-loaded.
        /// </summary>
        /// <param name="bytes">The bytes to pre-load.</param>
        public DataStream(byte[] bytes)
            : base(bytes)
        {
        }

        /// <summary>
        /// Constructs an empty data stream.
        /// </summary>
        public DataStream()
            : base()
        {
        }

        public DataStream(int capacity)
            : base(capacity)
        {
        }
    }
}
