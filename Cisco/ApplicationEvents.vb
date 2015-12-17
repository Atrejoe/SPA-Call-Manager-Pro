
Imports Microsoft.VisualBasic.ApplicationServices
Namespace My

    Partial Friend Class MyApplication

        '' <summary>
        '' License Validation Parameters copied from License Tracker 
        '' </summary>
        Public Const LICENSE_PARAMETERS As String = _
         "<AuthenticatedLicenseParameters>" + _
         "  <EncryptedLicenseParameters>" + _
         "    <ProductName>SPA Call Manager Pro</ProductName>" + _
         "    <RSAKeyValue>" + _
         "      <Modulus>1h9/qfIJwBfY2kupJ+2OD0KdfckM72E/6jQS9eCxa5NYooiVRwO97WBNEYxlTMAAof31CoN8ypI4I54fhRoIl34ybAJTd7DCgEQMnQqRd52oI/y2Le8vGDQKQ7cfowVGE8TMuOfbtc9k9YcEu3ZL6xmV3ziJVJy8Jxex7ohEqFE=</Modulus>" + _
         "      <Exponent>AQAB</Exponent>" + _
         "    </RSAKeyValue>" + _
         "    <DesignSignature>qSWYg3YEVQxNkVqBpuj5R083/rWN99MoFlILBA1auUYSypqFTu19uLbwe1XFctwIoOkKp2KjO3xku5m7bAHOzg77An7ElEOoNOrFO2uB+f3R44bQYpWdpFzi2dt+3iFwuXtcs7HmnpNv4foztFqqJN/8tfr8wJWIj401yjMWfRo=</DesignSignature>" + _
         "    <RuntimeSignature>rE/xMYVtUQHVo539G3Bh4rJ7tBGqhiiQJBOiilivZ8tOeMkGKE54blUVwxzOWsELGunGQzrQ0Zz/Ps/6Asw8BuS3K4LMS+ECAmguGF5PVgKIp1OsB7TEizXXs+yiA4qZcxAadHvBKbZzbhKKKnlfwP7GKRhzB6qZhcd4+K7xpJM=</RuntimeSignature>" + _
         "    <KeyStrength>7</KeyStrength>" + _
         "  </EncryptedLicenseParameters>" + _
         "  <AuthenticationServerURL>http://auth.spacallmanager.com/AuthenticationService.asmx</AuthenticationServerURL>" + _
         "  <ServerRSAKeyValue>" + _
         "    <Modulus>4P/IqIAbAf9eJ2zCMBOLG/eQAFlw0KJZqSMrYgh5FwoL4Z7/OWIBfYJMIMN1/I8JA6cLHnvMcaEfEIbHuyma3qzSB8ITkg05sxV6qAmJURXVDjGQDZ/1uTLi2D80mZg+YJFOdGu7ocQ54cjsexhKu2pQszdoXYDQISw67KmXP28=</Modulus>" + _
         "    <Exponent>AQAB</Exponent>" + _
         "  </ServerRSAKeyValue>" + _
         "</AuthenticatedLicenseParameters>"


        '' <summary>
        '' The name of the file to store the license key in - the sub directory is created
        '' automatically by ILS
        '' </summary>
        Private Shared _licenseFile As String = _
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\AuthenticatedApp\AuthenticatedApp.lic"

        '' <summary>
        '' The installed license if any
        '' </summary>
        Private Shared _license As AuthenticatedLicense

        '' <summary>
        '' Check the license at startup
        '' </summary>
        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As StartupEventArgs) Handles Me.Startup

            Dim provider As New AuthenticatedLicenseProvider
            _license = provider.GetLicense(LICENSE_PARAMETERS, _licenseFile, True)

            ' if there is no installed license then display the evaluation dialog until
            ' the user installs a license or selects Exit or Continue
            '
            '' '' ''If _license Is Nothing Then

            '' '' ''    Dim licenseForm As New AuthenticatedLicenseInstallForm
            '' '' ''    _license = licenseForm.ShowDialog(_licenseFile, _license)

            '' '' ''    If _license Is Nothing Then
            '' '' ''        Me.HideSplashScreen()
            '' '' ''        e.Cancel = True
            '' '' ''    End If
            '' '' ''End If

            While _license Is Nothing
                Dim evaluationMonitor As New RegistryEvaluationMonitor("ChopChop")
                Dim evaluationDialog As New EvaluationDialog(evaluationMonitor)
                evaluationDialog.TopMost = True ' ensure the dialog appears on top of the splash screen
                evaluationDialog.TrialDays = 7
                evaluationDialog.ExtendedTrialDays = 0
                Dim dialogResult As EvaluationDialogResult = evaluationDialog.ShowDialog()
                    Select dialogResult
                    Case EvaluationDialogResult.Continue
                        Exit While
                    Case EvaluationDialogResult.Exit
                        Me.HideSplashScreen()
                        e.Cancel = True
                        Exit While
                    Case EvaluationDialogResult.InstallLicense
                        Dim licenseForm As New AuthenticatedLicenseInstallForm
                        _license = licenseForm.ShowDialog(_licenseFile, _license)
                End Select
            End While


        End Sub

    End Class

End Namespace
