using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKWPF
{
    struct LabVertex
    {
        public Vector3 cylinderPosition;
        public Vector3 spherePosition;
        public Vector3 cylinderNormal;
        public Vector3 sphereNormal;
        public Vector2 textureCoorditate;

        public const int Size = (3 + 3 + 3 + 3 + 2) * 4;

        public LabVertex(Vector3 pos1, Vector3 pos2, Vector3 n1, Vector3 n2, Vector2 tex)
        {
            this.cylinderPosition = pos1;
            this.spherePosition = pos2;
            this.cylinderNormal = n1;
            this.sphereNormal = n2;
            this.textureCoorditate = tex;
        }
    }
}
