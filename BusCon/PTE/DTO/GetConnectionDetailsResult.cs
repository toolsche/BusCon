using System;

namespace BusCon.PTE.DTO
{
    public sealed class GetConnectionDetailsResult
    {
        public readonly DateTime currentDate;
        public readonly Connection connection;

        public GetConnectionDetailsResult(DateTime currentDate, Connection connection)
        {
            this.currentDate = currentDate;
            this.connection = connection;
        }

        public override string ToString()
        {
            return this.currentDate.ToString("dd.MM.yy") + "|" + this.connection.ToString();
        }
    }
}
