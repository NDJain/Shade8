using OpenTK;
using OpenTK.Graphics;

namespace OpenTKWPF
{
    struct NormalTexturedVertex
    {
        public const int Size = (3 + 3 + 3 + 3 + 2) * 4; // size of struct in bytes

        private readonly Vector3 position;
        private readonly Vector3 position2;

        private readonly Vector3 normal;
        private readonly Vector3 normal2;

        private readonly Vector2 tex;

        public NormalTexturedVertex(Vector3 position, Vector3 position2, Vector3 normal, Vector3 normal2, Vector2 tex)
        {
            this.position = position;
            this.position2 = position2;
            this.normal = normal;
            this.normal2 = normal2;
            this.tex = tex;
        }
    }
}
