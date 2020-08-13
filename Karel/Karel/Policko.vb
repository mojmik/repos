Structure Policko
	''' <summary>
	''' Počet značek na políčku
	''' </summary>
	Public pocetZnacek As Integer

	''' <summary>
	''' Typ políčka
	''' </summary>
	Public TypPolicka As TypyPolicka

	''' <summary>
	''' Konstruktor, nastaví typ políčka a počet značek
	''' </summary>
	''' <param name="typ"></param>
	''' <remarks></remarks>
	Public Sub New(typ As TypyPolicka)
		pocetZnacek = 0
		TypPolicka = typ
	End Sub

	''' <summary>
	''' Možné typy políčka
	''' </summary>
	Public Enum TypyPolicka
		nic
		zed
		Karel
	End Enum

	''' <summary>
	''' Nastaví poličku pokud je to možné buď zeď nebo nic
	''' </summary>
	Public Sub zedNeboNic()
		If Me.TypPolicka = Policko.TypyPolicka.nic Then
			Me.TypPolicka = Policko.TypyPolicka.zed
		ElseIf Me.TypPolicka = Policko.TypyPolicka.zed Then
			Me.TypPolicka = Policko.TypyPolicka.nic
		End If
	End Sub
End Structure
