// mailer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <mapi.h>
#include <mapix.h>

int _tmain(int argc, _TCHAR* argv[])
{
   MapiMessage  message;
   MapiFileDesc fileDesc[2];
   char         NoteText[8162];
   FILE         *fNote;
   MapiRecipDesc recip;

   ::CoInitialize(NULL);
   HMODULE hDll = ::LoadLibraryA("MAPI32.DLL");
   
   if (hDll == NULL)
      return MAPI_E_NOT_SUPPORTED; 

   ULONG (PASCAL *lpfnSendMail)(ULONG, ULONG, MapiMessage*, FLAGS, ULONG);
   (FARPROC&)lpfnSendMail = GetProcAddress(hDll, "MAPISendMail");
   if (lpfnSendMail == NULL)
      return MAPI_E_NOT_SUPPORTED; 

   // --- prepare the message ---

   memset(&message, 0, sizeof(message));
   message.lpszSubject  = (char*)argv[1];
   message.lpszNoteText = (char*)argv[2];
   if (strlen(argv[3]))
   {
      memset(&recip,0,sizeof(recip));
      recip.ulRecipClass = MAPI_TO;
      recip.lpszName = (char*)argv[3];
      message.lpRecips = &recip;
      message.nRecipCount = 1;
   }

   // --- prepare the attachments ---

   memset(&fileDesc[0], 0, sizeof(fileDesc[0]));
   fileDesc[0].nPosition = (ULONG)-1;
   fileDesc[0].lpszPathName = argv[4];
   fileDesc[0].lpszFileName = argv[5];

   if (argc > 7)
   {
      memset(&fileDesc[1], 0, sizeof(fileDesc[1]));
      fileDesc[1].nPosition = (ULONG)-1;
      fileDesc[1].lpszPathName = argv[6];
      fileDesc[1].lpszFileName = argv[7];
      message.nFileCount = 2;
      fNote = fopen(argv[8], "r");
   }
   else
   {
      message.nFileCount = 1;
      fNote = fopen(argv[6], "r");
   }

   message.lpFiles = fileDesc;

   size_t s = fread(&NoteText, 1, sizeof(NoteText), fNote);
   fclose(fNote);
   NoteText[min(s, sizeof(NoteText) - 1)] = '\0';
   message.lpszNoteText = (LPSTR) &NoteText;

   message.lpFiles = fileDesc;

   MAPIInitialize(NULL);
   int iRet = lpfnSendMail(0, NULL, &message, MAPI_NEW_SESSION|MAPI_DIALOG, 0);
   MAPIUninitialize();
   return iRet;
}

