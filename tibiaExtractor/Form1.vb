﻿Imports System.ComponentModel
Imports System.IO
Imports System.Threading
Imports System.Xml
Imports OpenTibia

Public Class Form1
    Dim oc As Client.IClient
    Dim thread As Thread
    Dim imlist As ImageList
    Dim xmldoc As XmlDocument
    Dim multiArray As List(Of ListitemObject) = New List(Of ListitemObject)

    Private Sub SettingsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SettingsToolStripMenuItem.Click
        settings.Show()

    End Sub

    Private Sub ExtractSpritesheetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExtractSpritesheetToolStripMenuItem.Click
        Dim work = New Worker(Label1)
        thread = New Thread(AddressOf work.Start)
        thread.Start()
        Label1.Text = "Done"
    End Sub

    Private Sub SliceSpritesheatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SliceSpritesheatToolStripMenuItem.Click
        Dim work As Worker = New Worker(Label1)
        Dim tileSize As Size = New Size(64, 64)
        Dim Offset As Point = New Point(0, 0)
        Dim Space As Size = New Size(0, 0)
        imlist = New ImageList()
        Dim filelist As String() = Directory.GetFiles(My.MySettings.Default.ExtractFolder + "\", "*.bmp")
        For i As Integer = 0 To filelist.Length - 1
            Dim spritesheet As Image = Image.FromFile(filelist(i))
            imlist = work.GenerateTileSetImageList(spritesheet, tileSize, Offset, Space)
        Next
        imlist.ImageSize = New Size(32, 32)
        For i As Integer = 0 To imlist.Images.Count - 1

            imlist.Images(i).Save(My.MySettings.Default.ExtractFolder + "\" + i.ToString + "_sprite" + ".png")

        Next
        Label1.Text = "Done"
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If IsNothing(False) Then
            thread.Abort()
        End If

    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

    End Sub

    Private Sub BakeASprToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BakeASprToolStripMenuItem.Click
        Panel1.Visible = True
        loadList()
    End Sub

    Private Sub loadList()
        xmldoc = New XmlDocument()
        Dim fs As New FileStream(CurDir() + "\clients.xml", FileMode.Open, FileAccess.Read)
        xmldoc.Load(fs)
        Dim Nodes As XmlNode = xmldoc.SelectSingleNode("/clients")
        For i As Integer = 0 To Nodes.ChildNodes.Count - 1
            Dim a As String = Nodes.ChildNodes(i).Attributes("description").Value
            Dim lob As ListitemObject = New ListitemObject()

            lob.description = Nodes.ChildNodes(i).Attributes("description").Value
            lob.version = CUShort(Nodes.ChildNodes(i).Attributes("version").Value)
            lob.otbversion = CUInt("&H" + Nodes.ChildNodes(i).Attributes("otbversion").Value)
            lob.sprsignature = CUInt("&H" + Nodes.ChildNodes(i).Attributes("sprsignature").Value)
            lob.datsignature = CUInt("&H" + Nodes.ChildNodes(i).Attributes("datsignature").Value)
            multiArray.Add(lob)

            ListBox1.Items.Add(a)
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim filelist As String() = Directory.GetFiles(My.MySettings.Default.ExtractFolder + "\", "*.png")

        Dim reader As New System.IO.BinaryReader(System.IO.File.OpenRead(My.MySettings.Default.Datfile))
        Dim DatVersion = reader.ReadUInt32()
        Dim DatVersionHex = Hex(DatVersion)
        reader.Close()
        reader.Dispose()

        Dim clientname As String = Nothing
        Dim version As UShort
        Dim otb As UInteger
        Dim spri As UInteger
        Dim dat As UInteger

        Dim selectedValues As List(Of ListitemObject)
        selectedValues = multiArray.FindAll(Function(p) p.datsignature = CUInt("&H" + DatVersionHex))
        If selectedValues.Count > 0 Then

            MessageBox.Show(selectedValues(0).description + " selected")
            clientname = selectedValues(0).description
            version = selectedValues(0).version
            otb = selectedValues(0).otbversion
            spri = selectedValues(0).sprsignature
            dat = selectedValues(0).datsignature

        End If
        Dim ver As Core.Version = New OpenTibia.Core.Version(version, clientname, dat, spri, otb)
        Dim osprite As OpenTibia.Client.Sprites.SpriteStorage = Client.Sprites.SpriteStorage.Load(My.MySettings.Default.TibiaFolder + "\Tibia.spr", ver)
        osprite.Version = ver
        Dim spr As Client.Sprites.Sprite = New Client.Sprites.Sprite()

        For i As Integer = 0 To filelist.Count - 1

            Dim im = Image.FromFile(filelist(i))

            spr.SetBitmap(im)
            spr.ID = osprite.Count + 1
            osprite.AddSprite(spr)

            ListBox2.Items.Add("created sprite with Id " + spr.ID.ToString)
        Next
        osprite.Save(My.MySettings.Default.ExtractFolder + "\Tibia.spr", ver)
        Label1.Text = "Done"
    End Sub

    Private Sub BakeADatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BakeADatToolStripMenuItem.Click
        Panel1.Visible = True
        loadList()

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim reader As New System.IO.BinaryReader(System.IO.File.OpenRead(My.MySettings.Default.Datfile))
        Dim DatVersion = reader.ReadUInt32()
        Dim DatVersionHex = Hex(DatVersion)
        reader.Close()
        reader.Dispose()

        Dim clientname As String = Nothing
        Dim version As UShort
        Dim otb As UInteger
        Dim spri As UInteger
        Dim dat As UInteger

        Dim selectedValues As List(Of ListitemObject)
        selectedValues = multiArray.FindAll(Function(p) p.datsignature = CUInt("&H" + DatVersionHex))
        If selectedValues.Count > 0 Then

            MessageBox.Show(selectedValues(0).description + " selected")
            clientname = selectedValues(0).description
            version = selectedValues(0).version
            otb = selectedValues(0).otbversion
            spri = selectedValues(0).sprsignature
            dat = selectedValues(0).datsignature
        End If

        Dim ver As Core.Version = New OpenTibia.Core.Version(version, clientname, dat, spri, otb)

        oc = New Client.ClientImpl()
        oc.Load(My.MySettings.Default.Datfile, My.MySettings.Default.Spritefile, ver)
        'Dim thingsto = oc.Things.GetThing(100, Client.Things.ThingCategory.Outfit)
        Dim thingsto = oc.Things.GetItem(100).Clone()

        thingsto.ID = 6000
        Dim spr = New Client.Sprites.Sprite()
        spr.ID = 6000
        Dim im As Image = Image.FromFile(My.MySettings.Default.ExtractFolder + "\0_sprite.png")
        spr.SetBitmap(im)

        oc.Sprites.AddSprite(spr)
        oc.Things.AddThing(thingsto)

        oc.Save(CurDir() + "\extracted\Tibia.dat", CurDir() + "\extracted\Tibia.spr", ver)
        MessageBox.Show("saved")

    End Sub
    Private Sub Second() 'test trying out stuff what works,apperently both throws an missing flags when loading up dat in objeectbuilder
        Dim reader As New System.IO.BinaryReader(System.IO.File.OpenRead(My.MySettings.Default.Datfile))
        Dim DatVersion = reader.ReadUInt32()
        Dim DatVersionHex = Hex(DatVersion)
        reader.Close()
        reader.Dispose()

        Dim clientname As String = Nothing
        Dim version As UShort
        Dim otb As UInteger
        Dim spri As UInteger
        Dim dat As UInteger

        Dim selectedValues As List(Of ListitemObject)
        selectedValues = multiArray.FindAll(Function(p) p.datsignature = CUInt("&H" + DatVersionHex))
        If selectedValues.Count > 0 Then

            MessageBox.Show(selectedValues(0).description + " selected")
            clientname = selectedValues(0).description
            version = selectedValues(0).version
            otb = selectedValues(0).otbversion
            spri = selectedValues(0).sprsignature
            dat = selectedValues(0).datsignature
        End If

        Dim ver As Core.Version = New OpenTibia.Core.Version(version, clientname, dat, spri, otb)
        Dim thingstore As OpenTibia.Client.Things.ThingTypeStorage = Client.Things.ThingTypeStorage.Load(My.MySettings.Default.TibiaFolder + "\Tibia.dat", ver)
        thingstore.Version = ver
        Dim thingsto1 = thingstore.GetItem(100)
        thingsto1.ID = 6000
        thingstore.AddThing(thingsto1)
        thingstore.Save(CurDir() + "\extracted\Tibia1.dat", ver)
        MessageBox.Show("saved")
    End Sub
End Class
