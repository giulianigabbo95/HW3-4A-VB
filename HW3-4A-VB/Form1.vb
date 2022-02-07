Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace CovidStatistics
    Public Partial Class Form1
        Inherits System.Windows.Forms.Form

        Private covidStats As System.Collections.Generic.List(Of CovidStatistics.CovidStat) = New System.Collections.Generic.List(Of CovidStatistics.CovidStat)()
        Private dataLoaded As Boolean

        Public Sub New()
            Me.InitializeComponent()
        End Sub

        ' Eventi

        Private Sub btnLoadCSV_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            Me.LoadCSV()
            Me.UpdateCombo()
        End Sub

        Private Sub btnMean_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            If Me.dataLoaded Then
                Me.txtMean.Text = "$ " & System.Math.Round(Me.CalcMean(), CInt((2))).ToString("#,##0.00")
            Else
                Call System.Windows.Forms.MessageBox.Show("Caricare il file CSV")
            End If
        End Sub

        Private Sub btnStdDeviation_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            If Me.dataLoaded Then
                Me.txtStdDeviation.Text = "$ " & System.Math.Round(Me.CalcStdDeviation(), CInt((2))).ToString("#,##0.00")
            Else
                Call System.Windows.Forms.MessageBox.Show("Caricare il file CSV")
            End If
        End Sub

        Private Sub btnFreqByComm_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            Me.txtFrequency.Text = String.Empty

            If Me.dataLoaded Then
                Dim values As System.Collections.Generic.List(Of System.Tuple(Of String, Double)) = Me.CalcFrequencyByCommodity()

                For Each item In values
                    Me.txtFrequency.Text += System.Math.Round(item.Item2, CInt((2))).ToString(CStr(("###,##0"))).PadLeft(7) & " Items" & Global.Microsoft.VisualBasic.Constants.vbTab & item.Item1 & Global.Microsoft.VisualBasic.Constants.vbCrLf
                Next
            Else
                Call System.Windows.Forms.MessageBox.Show("Caricare il file CSV")
            End If
        End Sub

        Private Sub btnFreByYear_Click(ByVal sender As Object, ByVal e As System.EventArgs)
            Me.txtFrequency.Text = String.Empty

            If Me.dataLoaded Then
                Dim values As System.Collections.Generic.List(Of System.Tuple(Of String, Double)) = Me.CalcFrequencyByYear()

                For Each item In values
                    Me.txtFrequency.Text += System.Math.Round(item.Item2, CInt((2))).ToString(CStr(("###,##0"))).PadLeft(7) & " Items" & Global.Microsoft.VisualBasic.Constants.vbTab & item.Item1 & Global.Microsoft.VisualBasic.Constants.vbCrLf
                Next
            Else
                Call System.Windows.Forms.MessageBox.Show("Caricare il file CSV")
            End If
        End Sub

        ' Metodi

        Private Sub LoadCSV()
            Dim data As String()
            Dim dataLine As String
            Dim lines = System.IO.File.ReadAllLines("Dati\effects-of-covid-19-on-trade.csv").Skip(1)

            For Each line In lines

                Try

                    If Not String.IsNullOrWhiteSpace(line) Then
                        dataLine = line
                        ' Rintraccia i campi delimitati da doppi apici (es.: "Milk powder, butter, and cheese") che contengono virgole e
                        ' rimpiazza le virgole con punto e virgola
                        While dataLine.Contains(""""c)
                            Dim p1 As Integer = line.IndexOf(""""c)
                            Dim p2 As Integer = line.IndexOf(""""c, p1 + 1)
                            dataLine = line.Substring(0, p1) & line.Substring(CInt((p1 + 1)), CInt((p2 - p1 - 1))).Replace(","c, ";"c) & line.Substring(p2 + 1)
                        End While

                        data = dataLine.Split(","c)

                        ' Aggiunge un elemento della classe alla collezione
                        Me.covidStats.Add(New CovidStatistics.CovidStat() With {
                            .Direction = data(CInt((0))).Trim(),
                            .Year = Integer.Parse(data(CInt((1))).Trim()),
                            .[Date] = System.DateTime.Parse(data(CInt((2))).Trim()),
                            .Weekday = data(CInt((3))).Trim(),
                            .Country = data(CInt((4))).Trim(),
                            .Commodity = data(CInt((5))).Trim(),
                            .Transport_Mode = data(CInt((6))).Trim(),
                            .Measure = data(CInt((7))).Trim(),
                            .Value = Double.Parse(data(CInt((8))).Trim()),
                            .Cumulative = Double.Parse(data(CInt((9))).Trim())
                        })
                        Me.dataLoaded = True
                    End If

                Catch ex As System.Exception
                    Call System.Windows.Forms.MessageBox.Show($"Errore.

Error message: {ex.Message}

" & $"Details:

{ex.StackTrace}")
                End Try
            Next
        End Sub

        Private Sub UpdateCombo()
            If Me.dataLoaded Then
                Dim countries = Me.covidStats.GroupBy(Function(x) x.Country).[Select](Function(x) x.FirstOrDefault()).ToList()
                Me.cmbCountries.Items.Clear()

                For Each item In countries
                    Me.cmbCountries.Items.Add(item.Country)
                Next

                Me.cmbCountries.SelectedIndex = 0
            End If
        End Sub

        Private Function CalcMean() As Double
            Dim mean As Double = 0

            ' Filtra l'elenco in base alle selezioni in maschera
            Dim stats = Me.covidStats.Where(Function(x) Equals(x.Country, Me.cmbCountries.SelectedItem.ToString())).ToList()

            If stats.Any() Then
                For Each covidStat As CovidStatistics.CovidStat In stats
                    mean += covidStat.Value
                Next

                mean = mean / stats.Count()
            End If

            Return mean
        End Function

        Private Function CalcStdDeviation() As Double
            Dim stdDeviation As Double = 0

            ' Filtra l'elenco in base alle selezioni in maschera
            Dim stats = Me.covidStats.Where(Function(x) Equals(x.Country, Me.cmbCountries.SelectedItem.ToString())).ToList()
            Dim values = New System.Collections.Generic.List(Of Double)()

            For Each covidStat As CovidStatistics.CovidStat In stats
                values.Add(covidStat.Value)
            Next

            If values.Any() Then
                Dim avg As Double = values.Average()
                Dim sum As Double = values.Sum(Function(d) System.Math.Pow(d - avg, 2))
                stdDeviation = System.Math.Sqrt((sum) / (values.Count() - 1))
            End If

            Return stdDeviation
        End Function

        Private Function CalcFrequencyByCommodity() As System.Collections.Generic.List(Of System.Tuple(Of String, Double))
            Dim values As System.Collections.Generic.List(Of System.Tuple(Of String, Double)) = New System.Collections.Generic.List(Of System.Tuple(Of String, Double))()

            ' Filtra l'elenco in base alle selezioni in maschera
            Dim commodities = Me.covidStats.GroupBy(Function(x) x.Commodity).[Select](Function(x) x.FirstOrDefault()).ToList()

            For Each cmd In commodities
                Dim stats = Me.covidStats.Where(Function(x) Equals(x.Country, Me.cmbCountries.SelectedItem.ToString()) AndAlso Equals(x.Commodity, cmd.Commodity)).ToList()
                values.Add(New System.Tuple(Of String, Double)(cmd.Commodity, stats.Count()))
            Next

            Return values
        End Function

        Private Function CalcFrequencyByYear() As System.Collections.Generic.List(Of System.Tuple(Of String, Double))
            Dim values As System.Collections.Generic.List(Of System.Tuple(Of String, Double)) = New System.Collections.Generic.List(Of System.Tuple(Of String, Double))()

            ' Filtra l'elenco in base alle selezioni in maschera
            Dim years = Me.covidStats.GroupBy(Function(x) x.Year).[Select](Function(x) x.FirstOrDefault()).ToList()

            For Each value In years
                Dim stats = Me.covidStats.Where(Function(x) Equals(x.Country, Me.cmbCountries.SelectedItem.ToString()) AndAlso x.Year = value.Year).ToList()
                values.Add(New System.Tuple(Of String, Double)(value.Year.ToString(), stats.Count()))
            Next

            Return values
        End Function
    End Class
End Namespace
