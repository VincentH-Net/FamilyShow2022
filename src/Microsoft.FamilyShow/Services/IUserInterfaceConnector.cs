namespace Microsoft.FamilyShow.Services;

public interface IUserInterfaceConnector
{
    void BuildOpenMenu();
    void ChangeSkin(string skinName);
    void ExportDiagram(string fileName);
    void HideFamilyDataControl();
    void HideOldVersion();
    void PromptToSave();
    void ShowDetailsPane();
    void ShowFamilyDataControl();
    void ShowNewUserControl();
    void ShowOldVersion();
    void ShowWelcomeScreen();
}