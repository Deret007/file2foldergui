﻿Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions

Public Class Form1

    Dim moveItems As New BindingList(Of MoveItem)
    Dim isUndo As Boolean = False

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        FolderBrowserDialog1.ShowDialog()
        txtBoxDir.Text = FolderBrowserDialog1.SelectedPath
    End Sub

    Private Sub btnMove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMove.Click

        If String.IsNullOrEmpty(txtBoxDir.Text.Trim) = False AndAlso IO.Directory.Exists(txtBoxDir.Text) Then
            btnUndo.Enabled = False
            btnMove.Enabled = False
            isUndo = False
            ProgressBar1.Value = 0
            bgwMover.RunWorkerAsync()
        Else
            txtBoxDir.Text = "Choose a valid directory!"
        End If
    End Sub

    Private Sub btnUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUndo.Click
        btnUndo.Enabled = False
        btnMove.Enabled = False
        isUndo = True
        ProgressBar1.Value = 0
        bgwMover.RunWorkerAsync()
    End Sub

    Private Sub bgwMover_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgwMover.DoWork
        If isUndo = False Then
            Dim files() As String = IO.Directory.GetFiles(txtBoxDir.Text.Trim)
            If files.Length > 0 Then moveItems.Clear()
            Dim i As Integer = 1
            For Each filePath As String In files
                Dim fi As New FileInfo(filePath)
                If (fi.Attributes And IO.FileAttributes.Hidden Or FileAttributes.System) = IO.FileAttributes.Hidden Or IO.FileAttributes.System Or IO.File.Exists("file2foldergui.exe") Then Continue For
                Try
                    Dim newFolderPath As String = IO.Path.Combine(txtBoxDir.Text.Trim, IO.Path.GetFileNameWithoutExtension(filePath))
                    If Not IO.Directory.Exists(newFolderPath) Then
                        IO.Directory.CreateDirectory(newFolderPath)
                    End If

                    Dim mi As New MoveItem
                    mi.OldPath = filePath
                    mi.NewPath = IO.Path.Combine(newFolderPath, IO.Path.GetFileName(filePath))
                    moveItems.Add(mi)

                    bgwMover.ReportProgress((i / files.Length) * 100, "Moving """ & IO.Path.GetFileName(mi.OldPath) & """ to """ & mi.NewPath & """...")
                    IO.File.Move(mi.OldPath, mi.NewPath)
                    bgwMover.ReportProgress(0, "Done." & vbCrLf)
                    i += 1
                Catch ex As Exception
                    bgwMover.ReportProgress(0, "Error: " & ex.Message & vbCrLf)
                End Try
            Next
        Else
            Dim i As Integer = 1
            For Each mi As MoveItem In moveItems
                Try
                    bgwMover.ReportProgress((i / moveItems.Count) * 100, "Undoing """ & IO.Path.GetFileName(mi.NewPath) & """ to """ & mi.OldPath & """...")
                    IO.File.Move(mi.NewPath, mi.OldPath)
                    IO.Directory.Delete(My.Computer.FileSystem.GetParentPath(mi.NewPath))
                    bgwMover.ReportProgress(0, "Done." & vbCrLf)
                    i += 1
                Catch ex As Exception
                    bgwMover.ReportProgress(0, "Error: " & ex.Message & vbCrLf)
                End Try
            Next
        End If
    End Sub
    Private Sub bgwMover_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles bgwMover.ProgressChanged
        If e.ProgressPercentage <> 0 Then
            ProgressBar1.Value = e.ProgressPercentage
        End If
        If Not e.UserState Is Nothing Then
            txtLog.Text &= e.UserState
        End If
    End Sub
    Private Sub bgwMover_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgwMover.RunWorkerCompleted
        ProgressBar1.Value = 100
        btnMove.Enabled = True
        btnUndo.Enabled = True
    End Sub
    Private Sub SaveLogToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveLogToolStripMenuItem.Click
        SaveFileDialog1.ShowDialog()
        Try
            System.IO.File.WriteAllText(SaveFileDialog1.FileName, txtLog.Text)
        Catch ex As Exception
            System.IO.File.WriteAllText(0, "Error: " & ex.Message & vbCrLf)
        End Try
    End Sub

    Private Sub ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem1.Click
        AboutBox1.Show()
    End Sub

    Private Sub CheckForUpdateToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckForUpdateToolStripMenuItem.Click
        Dialog1.Show()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim url As New System.Uri("http://update.thehtpc.net/file2foldergui/UpdateVersion.txt")
        Dim req As WebRequest
        req = WebRequest.Create(url)
        Dim resp As WebResponse
        resp = req.GetResponse()
        resp.Close()
        req = Nothing
        webBrwsStartup.Navigate("http://update.thehtpc.net/file2foldergui/UpdateVersion.txt")
    End Sub

    Public Sub webBrwsStartup_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles webBrwsStartup.DocumentCompleted
        Dim m As Match = Regex.Match(webBrwsStartup.DocumentText, "<PRE>(?<version>(.*?))</PRE>")
        If m.Success = True Then
            If Application.ProductVersion <> m.Groups("version").Value Then
                Form2.Show()
            End If
        End If
    End Sub
End Class

Public Class MoveItem
    Dim _oldPath As String = String.Empty
    Public Property OldPath() As String
        Get
            Return _oldPath
        End Get
        Set(ByVal value As String)
            _oldPath = value
        End Set
    End Property

    Dim _newPath As String = String.Empty
    Public Property NewPath() As String
        Get
            Return _newPath
        End Get
        Set(ByVal value As String)
            _newPath = value
        End Set
    End Property
End Class