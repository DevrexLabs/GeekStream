using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class RegisterClickCommand : Command<GeekStreamModel>
    {
        public readonly long Id;
        public readonly string Query;

        public RegisterClickCommand(long id, string query)
        {
            Id = id;
            Query = query;
        }

        #region Overrides of Command<GeekStreamModel>

        protected override void Execute(GeekStreamModel model)
        {
            model.Click(Id, Query);
        }

        #endregion
    }
}
