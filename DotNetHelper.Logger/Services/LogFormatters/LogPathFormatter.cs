using System.Text;
using DotNetHelper.Logger.Interfaces;

namespace DotNetHelper.Logger.Services.LogFormatters
{
    public class LogPathFormatter : LogFormatter, ILogPathFormatter
    {
        public string RootPath { get; }

        public LogPathFormatter(string template) : base(template)
        {
            RootPath = GetRootFolderPath();
        }

        private string GetRootFolderPath()
        {
            if (string.IsNullOrWhiteSpace(Template))
                throw new ArgumentNullException(nameof(Template));

            bool isUnc = Template.StartsWith(@"\\", StringComparison.Ordinal);
            var sb = new StringBuilder();
            int index = 0;

            // UNC 면  \\server\share  를 무조건 Root 에 포함
            if (isUnc)
            {
                sb.Append(@"\\");
                // server
                int next = Template.IndexOf('\\', 2);
                if (next < 0) return Template; // 잘못된 UNC
                sb.Append(Template, 2, next - 2);
                // share
                int startShare = next + 1;
                next = Template.IndexOf('\\', startShare);
                if (next < 0) // 경로가 \\server\share 로 끝
                {
                    sb.Append('\\').Append(Template, startShare, Template.Length - startShare);
                    return sb.ToString();
                }
                sb.Append('\\').Append(Template, startShare, next - startShare);
                index = next + 1; // 이후부터 동적 검사
            }

            // 나머지 세그먼트: 동적 토큰 만나기 전까지만 덧붙임
            while (index < Template.Length)
            {
                int next = Template.IndexOf('\\', index);
                string segment = next < 0
                                 ? Template.Substring(index)
                                 : Template.Substring(index, next - index);

                if (IsDynamic(segment)) break;

                if (sb.Length > 0 && sb[sb.Length - 1] != Path.DirectorySeparatorChar)
                    sb.Append(Path.DirectorySeparatorChar);

                sb.Append(segment);

                if (next < 0) break; // 마지막 세그먼트
                index = next + 1;
            }

            return sb.ToString();
        }
    }
}
