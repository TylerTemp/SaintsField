using System.Reflection;

namespace SaintsField.Editor.Utils
{
    public readonly struct GetOfResult<T>
    {
        public readonly string Error;
        public readonly MemberInfo MemberInfo;
        public readonly T Result;
        public readonly bool EmptyPathResult;

        public GetOfResult(string error, MemberInfo memberInfo, T result, bool emptyPathResult)
        {
            Error = error;
            MemberInfo = memberInfo;
            Result = result;
            EmptyPathResult = emptyPathResult;
        }

        public void Deconstruct(out string error, out MemberInfo memberInfo, out T result)
        {
            error = Error;
            memberInfo = MemberInfo;
            result = Result;
        }
    }
}
