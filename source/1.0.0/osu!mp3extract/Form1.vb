Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        FolderBrowserDialog1.Reset()
        Dim r = FolderBrowserDialog1.ShowDialog()
        If r = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        FolderBrowserDialog1.Reset()
        Dim r = FolderBrowserDialog1.ShowDialog()
        If r = Windows.Forms.DialogResult.OK Then
            TextBox2.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub TextBox1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.Click
        Button1.PerformClick()
    End Sub

    Private Sub TextBox2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.Click
        Button2.PerformClick()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim osufolder = TextBox1.Text.ToString
        Dim tofolder = TextBox2.Text.ToString
        Dim songsok As New ArrayList
        If TextBox1.Text.Length = 0 Then
            MsgBox("Debe seleccionar la carpeta songs de osu!", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Exit Sub
        End If
        If TextBox2.Text.Length = 0 Then
            MsgBox("Debe seleccionar la carpeta de salida de los mp3", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Exit Sub
        End If
        If Not My.Computer.FileSystem.DirectoryExists(osufolder) Then
            MsgBox("La carpeta seleccionada no existe!", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly)
            Exit Sub
        End If
        If Not My.Computer.FileSystem.DirectoryExists(tofolder) Then
            My.Computer.FileSystem.CreateDirectory(tofolder)
        End If

        My.Settings.osufolder = osufolder
        My.Settings.tofolder = tofolder
        My.Settings.Save()
        Dim songsfolders = My.Computer.FileSystem.GetDirectories(osufolder)
        ProgressBar1.Maximum = songsfolders.Count
        For Each folder In songsfolders
            Dim files = My.Computer.FileSystem.GetFiles(folder)
            Dim listfileok As New ArrayList
            For Each file In files
                If file.ToLower.EndsWith(".osu") Then
                    Dim fileosu As New System.IO.StreamReader(file)
                    Dim ends = False
                    Dim songartist = ""
                    Dim songtitle = ""
                    Dim audiofile = ""
                    While ends = False
                        If fileosu.EndOfStream Then Exit While
                        Dim linea = fileosu.ReadLine

                        If linea.StartsWith("AudioFilename: ") Then
                            audiofile = linea.Remove(0, "AudioFilename: ".Length)
                        End If
                        If linea.StartsWith("Title:") Then
                            songtitle = linea.Remove(0, "Title:".Length)
                        End If
                        If linea.StartsWith("Artist:") Then
                            songartist = linea.Remove(0, "Artist:".Length)
                        End If
                    End While
                    fileosu.Close()

                    audiofile = Trim(audiofile)
                    songtitle = Trim(songtitle)
                    songartist = Trim(songartist)

                    If audiofile = "" Then Continue For
                    If listfileok.Contains(audiofile) Then Continue For

                    Dim newfilename As String = ""
                    If Not songartist = "" And Not songtitle = "" Then
                        newfilename = songartist & " - " & songtitle & audiofile.Substring(audiofile.LastIndexOf("."), audiofile.Length - audiofile.LastIndexOf("."))
                    Else
                        newfilename = audiofile
                    End If
                    listfileok.Add(audiofile)

                    Dim reg As New System.Text.RegularExpressions.Regex("[^a-zA-Z0-9\(\) \.\-_\^\!\[\]]")
                    newfilename = reg.Replace(newfilename, "")

                    'If My.Computer.FileSystem.FileExists(tofolder & "\" & newfilename) Then
                    'Continue For
                    'End If

                    Try
                        System.IO.File.Copy(folder & "\" & audiofile, tofolder & "\" & newfilename, True)
                    Catch ex As Exception

                    End Try


                    ' Dim prop As New DSOFile.OleDocumentProperties
                    'prop.Open(tofolder & "\" & newfilename)

                    If Not audiofile.Substring(audiofile.LastIndexOf("."), audiofile.Length - audiofile.LastIndexOf(".")).ToLower = ".mp3" Then
                        Continue For
                    End If
                    Dim mp3 As New MP3ID3v1(tofolder & "\" & newfilename)
                    If (mp3.TagExists) Then
                        Dim dirty As Boolean = False
                        If mp3.Frame(MP3ID3v1.FrameTypes.Title) = "" Then
                            mp3.Frame(MP3ID3v1.FrameTypes.Title) = songtitle
                            dirty = True
                        End If

                        If mp3.Frame(MP3ID3v1.FrameTypes.Artist) = "" Then
                            mp3.Frame(MP3ID3v1.FrameTypes.Title) = songartist
                            dirty = True
                        End If

                        If dirty Then mp3.Update()
                    Else
                        mp3.Frame(MP3ID3v1.FrameTypes.Title) = songtitle
                        mp3.Frame(MP3ID3v1.FrameTypes.Artist) = songartist
                        mp3.Frame(MP3ID3v1.FrameTypes.Track) = 0
                        mp3.Frame(MP3ID3v1.FrameTypes.Genre) = MP3ID3v1.Genres.Other
                        mp3.Update()
                    End If
                End If
            Next
            ProgressBar1.Value += 1
        Next
        MsgBox("Listo", MsgBoxStyle.Exclamation + MsgBoxStyle.OkOnly)
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Text = My.Settings.osufolder
        TextBox2.Text = My.Settings.tofolder
    End Sub
End Class
