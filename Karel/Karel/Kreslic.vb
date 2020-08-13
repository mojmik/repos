Class Kreslic
	''' <summary>
	''' Grafika, na tu budeme kreslit
	''' </summary>
	''' <remarks></remarks>
	Private g As Graphics

	''' <summary>
	''' Konstruktor, nastaví grafiku
	''' </summary>
	''' <param name="g">Grafika, na kterou se má vykreslovat</param>
	''' <remarks></remarks>
	Public Sub New(g As Graphics)
		Me.g = g
	End Sub

	''' <summary>
	''' Vykreslí políčko z mapy
	''' </summary>
	''' <param name="s">Souřadnice políčka</param>
	Public Sub vykresliPolicko(s As Souradnice)
		' Musíme si pohlídat záporné souřadnice
		Try
			' kařdopádně políčko vyčistíme
			g.FillRectangle(Brushes.White, s.x * 20, s.y * 20, 20, 20)
			' Vykreslíme buď nic, nebo zeď, ta musí být VŽDY v pozadí, 
			' značky je překreslí

			' Pořadí vykreslování:
			' 1.) zeď nebo nic
			' 2.) značky
			' 3.) Karel nebo nic

			' nic nekreslíme, protože políčko je stejně vyčistěné,
			' zeď vykreslíme
			If mapa(s.x, s.y).TypPolicka = Policko.TypyPolicka.zed Then
				g.DrawImage(My.Resources.wall, s.x * 20, s.y * 20, 20, 20)
			End If

			' Vykreslování značek
			If mapa(s.x, s.y).pocetZnacek <> 0 Then
				' cyklicky je vykreslíme
				For i = 0 To mapa(s.x, s.y).pocetZnacek
					' musíme si pohlídat, aby jich nebylo až moc. 
					' To by pak zasáhli do políčka pod tímto
					If i < 10 Then
						g.FillRectangle(Brushes.Yellow, s.x * 20,
										s.y * 20 + (i * 2), 20, 2)
					End If
				Next
			End If

			' Vykreslíme Karla
			If mapa(s.x, s.y).TypPolicka = Policko.TypyPolicka.Karel Then
				g.DrawImage(Karel.KarluvObrazek, s.x * 20, s.y * 20, 20, 20)
			End If
			' Zpracujeme události (zviditelníme změny).
			My.Application.DoEvents()
		Catch ex As Exception
			'Záporné souřadnice, konec světa to ale není.
		End Try
	End Sub

	''' <summary>
	''' Vykreslí políčka kolem Karla a Karla taky
	''' </summary>
	Sub VykresliPolickaKolemKarla()
		' Vezmeme souřadnice od Karla -1 až 1, vezmeme X i Y

		' X(-1; -1) X(0; -1) X(1; -1)
		' X(-1;  0) K(0;  0) X(1;  0)
		' X(-1;  1) X(0;  1) X(1;  1)
		' X = políčko
		' K = Karel

		' Vykreslíme tedy X a K (viz. předcozí komentář).


		For x = Karel.pozice.x - 1 To Karel.pozice.x + 1
			For y = Karel.pozice.y - 1 To Karel.pozice.y + 1
				vykresliPolicko(New Souradnice(x, y))
			Next
		Next
	End Sub

End Class
