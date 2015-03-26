# Signere register account
Klient progam for å etablere kundeforhold til Signere.no

Dette gjør det veldig lett og raskt å implementere Signere.no i dine løsninger og etablere kontoer for dine kunder.

Programmet er en windows exe fil som krever .NET 3.5 installert.


###Input parameters:
1. DealerID (Guid) din forhandler ID (må være Signere.no forhandler for å få dette
2. Register URL denne kan du få fra Signere.no salg, dette er en tilpasset web side med skjema for bestilling av konto. Kan flette inn informasjon til skjema via parametre i URL.
3. Filsti til lisesnfil eller json/xml fil (se pkt. 4 formater)
4. Format lic (lisensfil) Json (Tilgangsnøkler og kontoid i json format) Xml (samme som json bare i Xml format)

###Feil koder:
0.  Alt gikk bra
1.  Bruker avbrøt prossessen
2.  Feil med input paramtere
3. Generell feil
  * 31 Filsti i parameter 3 finnes allerede
  * 32 Feil med å hente ned lisensinformasjon fra API
