!include "MUI2.nsh"
!include "EnvVarUpdate.nsh"
; infragen.nsi
;

;--------------------------------

!define VERSION "1.3"

; The name of the installer
Name "Infragen"
Caption "Infragen ${VERSION} Setup"

; The file to write
OutFile "InfragenSetup.exe"

; The default installation directory
InstallDir $PROGRAMFILES\Infragen

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------
!define MUI_WELCOMEPAGE_TITLE "Welcome to the Infragen ${VERSION} setup wizard"
!define MUI_WELCOMEPAGE_TEXT "This wizard will guide you through the installation of Infragen ${VERSION}, the command-line schema generation tool for TIBCO ActiveMatrix Adapter for Infranet.$\r$\n$\r$\nThis version contains the generator itself and the converter of the existing schemas.$\r$\n$\r$\nNOTE: This software requires Microsoft .NET Framework 3.5 to be installed.$\r$\n$\r$\n$_CLICK"

!define MUI_FINISHPAGE_LINK "Visit the project site for the latest news, FAQs and support"
!define MUI_FINISHPAGE_LINK_LOCATION "http://enterprise-way.com/wiki/Infragen"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "license.txt"

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES


;--------------------------------
; Languages

!insertmacro MUI_LANGUAGE "English"

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important

  SetOverwrite on

  SetOutPath $INSTDIR
  File license.txt

  SetOutPath $INSTDIR\bin
  
  File bin\*.*
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\infragen" "DisplayName" "Infragen tool"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\infragen" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\infragen" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\infragen" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

SectionEnd ; end the section

Section "Add to Path"

  ${EnvVarUpdate} $0 "PATH" "A" "HKLM" "$INSTDIR\bin"

SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\infragen"

  ; Remove files and uninstaller
  Delete $INSTDIR\bin\*.*
  Delete $INSTDIR\*.*

  ; Remove directories used
  RMDir "$INSTDIR\bin"
  RMDir "$INSTDIR"

  ${un.EnvVarUpdate} $0 "PATH" "R" "HKLM" "$INSTDIR\bin"

SectionEnd
