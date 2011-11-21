using System.Text;

namespace BusCon.PTE.DTO
{
    public sealed class LineDestination
    {
        public readonly string line;
        public readonly int destinationId;
        public readonly string destination;

        public LineDestination(string line, int destinationId, string destination)
        {
            this.line = line;
            this.destinationId = destinationId;
            this.destination = destination;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Line(");
            stringBuilder.Append(this.line != null ? this.line : "null");
            stringBuilder.Append(",");
            stringBuilder.Append(this.destinationId);
            stringBuilder.Append(",");
            stringBuilder.Append(this.destination != null ? this.destination : "null");
            stringBuilder.Append(")");
            return ((object)stringBuilder).ToString();
        }

        public override bool Equals(object o)
        {
            if (o == this)
                return true;
            if (!(o is LineDestination))
                return false;
            LineDestination lineDestination = (LineDestination)o;
            if (!this.nullSafeEquals((object)this.line, (object)lineDestination.line) || this.destinationId != lineDestination.destinationId || !this.destination.Equals(lineDestination.destination))
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return ((0 + this.nullSafeHashCode((object)this.line)) * 29 + this.destinationId) * 29 + this.destination.GetHashCode();
        }

        private bool nullSafeEquals(object o1, object o2)
        {
            if (o1 == null && o2 == null || o1 != null && o1.Equals(o2))
                return true;
            else
                return false;
        }

        private int nullSafeHashCode(object o)
        {
            if (o == null)
                return 0;
            else
                return o.GetHashCode();
        }
    }
}
