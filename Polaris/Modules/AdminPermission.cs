using Polaris.Authorization;

namespace Polaris.Modules
{
    public class AdminPermission : IPermission
    {
        public string Identifier => "admin";

        public Operation PossibleOperations => Operation.Get | Operation.Set | Operation.Delete;

        public class Claims : IPermission
        {
            public string Identifier => "admin.claims";

            public Operation PossibleOperations => Operation.Get | Operation.Set | Operation.Delete;
        }
    }
}
