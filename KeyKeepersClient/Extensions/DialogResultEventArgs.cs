namespace KeyKeepersClient.ViewModels.CommunityViewModel;

public class DialogResultEventArgs : EventArgs
{
    public DialogResultEventArgs(bool dialogResult, string communityName = null!)
    {
        DialogResult = dialogResult;
        CommunityName = communityName;
    }

    public bool DialogResult { get; }

    public string CommunityName { get; }
}
