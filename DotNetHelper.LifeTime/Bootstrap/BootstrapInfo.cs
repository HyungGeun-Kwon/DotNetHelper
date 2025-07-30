namespace CvsSIService.LifeTime.Bootstrap
{
    public class BootstrapperInfo
    {
        public string Name { get; set; }         // 부트스트래퍼 제목
        public int ProgressPercent { get; set; }  // 진행률

        public BootstrapperInfo(string name, int progressPercent = 0)
        {
            Name = name;
            ProgressPercent = progressPercent;
        }

        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        /// <param name="totalCount">Bootstrapper 전체 수</param>
        /// <param name="nowIndex">현재 Index. 0부터 시작됨.</param>
        public void SetProgressPercent(int totalCount, int nowIndex)
        {
            ProgressPercent = nowIndex * 100 / totalCount;
        }
    }

}
