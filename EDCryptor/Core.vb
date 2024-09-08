'This module's imports and settings.
Option Compare Binary
Option Explicit On
Option Infer Off
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Convert
Imports System.Environment
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography

'This module contains this program's core procedures.
Public Module CoreModule
   'This structure defines an AES key and IV.
   Private Structure AESKeyIVStr
      Public AESKey As String  'Defines a base-64 encoded AES initialization vector.
      Public AESIV As String   'Defines a base-64 encoded AES key.
   End Structure

   'This procedure is executed when this program is started.
   Public Sub Main()
      Try
         Dim Arguments As New List(Of String)(GetCommandLineArgs())

         Arguments.RemoveAt(0)

         If Arguments.Count = 1 AndAlso Arguments.Last().Trim() = "/g" Then
            With GenerateAESKeyIV()
               Console.WriteLine(.AESKey)
               Console.WriteLine(.AESIV)
            End With
         ElseIf Arguments.Count = 2 Then
            Select Case Arguments.First.Trim()
               Case "/d"
                  Console.WriteLine(Decrypt(GetAESKeyIV(Arguments.Last.Trim()), Console.In.ReadToEnd()))
               Case "/e"
                  Console.WriteLine(Encrypt(GetAESKeyIV(Arguments.Last.Trim()), Console.In.ReadToEnd()))
               Case Else
                  DisplayHelp()
            End Select
         Else
            DisplayHelp()
         End If
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try
   End Sub

   'This procedure decrypts the specified data and returns the result.
   Private Function Decrypt(AESKeyIV As AESKeyIVStr, Encrypted As String) As String
      Try
         Dim AESO As Aes = Aes.Create()
         Dim Data As String = Nothing

         With New StreamReader(New CryptoStream(New MemoryStream(FromBase64String(Encrypted)), AESO.CreateDecryptor(FromBase64String(AESKeyIV.AESKey), FromBase64String(AESKeyIV.AESIV)), CryptoStreamMode.Read))
            Data = .ReadToEnd()
         End With

         Return Data
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try

      Return Nothing
   End Function

   'This procedure displays the help.
   Private Sub DisplayHelp()
      Try
         Console.WriteLine(ProgramInformation())
         Console.WriteLine()
         Console.WriteLine(My.Application.Info.Description)
         Console.WriteLine()
         Console.WriteLine("Command line options:")
         Console.WriteLine("/d KEY_FILE = decrypt using the specified key file.")
         Console.WriteLine("/e KEY_FILE = encrypt using the specified key file.")
         Console.WriteLine("/g = generate key and iv")
         Console.WriteLine()
         Console.WriteLine("Decrypting/encrypting:")
         Console.WriteLine("Data to be decrypted or encrypted is read from STDIN. The result is written to STDOUT.")
         Console.WriteLine()
         Console.WriteLine("Note: base-64 encoding is used for all I/O.")
         Console.WriteLine()
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try
   End Sub

   'This procedure encrypts the specified text and returns the result.
   Private Function Encrypt(AESKeyIV As AESKeyIVStr, Data As String) As String
      Try
         Dim AESO As Aes = Aes.Create()
         Dim Encrypted As New MemoryStream()

         Using Writer As New StreamWriter(New CryptoStream(Encrypted, AESO.CreateEncryptor(FromBase64String(AESKeyIV.AESKey), FromBase64String(AESKeyIV.AESIV)), CryptoStreamMode.Write))
            Writer.Write(Data)
         End Using

         Return ToBase64String(Encrypted.ToArray())
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try

      Return Nothing
   End Function

   'This procedure displays any errors that occur.
   Private Sub DisplayError(ExceptionO As Exception)
      Try
         Console.Error.WriteLine($"ERROR: {ExceptionO.Message}")
         [Exit](0)
      Catch
         [Exit](0)
      End Try
   End Sub

   'This procedure generates an AES key and iv and returns the result.
   Private Function GenerateAESKeyIV() As AESKeyIVStr
      Try
         Dim AESO As Aes = Aes.Create()

         AESO.GenerateKey()
         AESO.GenerateIV()

         Return New AESKeyIVStr With {.AESKey = ToBase64String(AESO.Key), .AESIV = ToBase64String(AESO.IV)}
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try

      Return Nothing
   End Function

   'This procedure reads an AES key and iv from the specified file and returns the result.
   Private Function GetAESKeyIV(PathO As String) As AESKeyIVStr
      Try
         Dim AESKeyIV As New AESKeyIVStr

         Using FileO As New StreamReader(PathO)
            With AESKeyIV
               .AESKey = FileO.ReadLine()
               .AESIV = FileO.ReadLine()
            End With
         End Using

         Return AESKeyIV
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try

      Return Nothing
   End Function

   'This procedure returns information about this program.
   Private Function ProgramInformation() As String
      Try
         With My.Application.Info
            Return $"{ .Title} v{ .Version} - by: { .CompanyName}, { .Copyright }"
         End With
      Catch ExceptionO As Exception
         DisplayError(ExceptionO)
      End Try

      Return Nothing
   End Function
End Module
