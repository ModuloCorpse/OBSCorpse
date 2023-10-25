using CorpseLib;

namespace OBSCorpse
{
    public class FileCountdownTimeAction : TimedAction
    {
        private readonly string m_FilePath;
        private readonly string m_FinishMessage;

        public FileCountdownTimeAction(string filePath, long durationInSeconds) : base(250, (durationInSeconds - 1) * 1000)
        {
            m_FilePath = filePath;
            m_FinishMessage = string.Empty;
        }

        public FileCountdownTimeAction(string filePath, string finishMessage, long durationInSeconds) : base(250, (durationInSeconds - 1) * 1000)
        {
            m_FilePath = filePath;
            m_FinishMessage = finishMessage;
        }

        protected override void OnActionStart()
        {
            base.OnActionStart();
            OnActionUpdate(0);
        }

        protected override void OnActionUpdate(long elapsed)
        {
            TimeSpan remainingTime = TimeSpan.FromMilliseconds((Duration + 1000) - elapsed);
            string remainingStr = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
            if (!string.IsNullOrEmpty(m_FilePath))
                File.WriteAllText(m_FilePath, remainingStr);
            base.OnActionUpdate(elapsed);
        }

        protected override void OnActionFinish()
        {
            if (!string.IsNullOrEmpty(m_FilePath) && !string.IsNullOrEmpty(m_FinishMessage))
                File.WriteAllText(m_FilePath, m_FinishMessage);
            base.OnActionFinish();
        }
    }
}
