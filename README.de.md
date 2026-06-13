# QWERTZ-Bridge (Deutsche Kurzanleitung)

**`<`, `>` und `|` auf einer ANSI-(US)-Tastatur mit deutschem QWERTZ-Layout tippen.**

Physischen ANSI-Tastaturen (z. B. Ducky One 3 SF) fehlt die ISO-102-Taste neben dem
linken Shift. Auf dem deutschen Layout ist sie die einzige Quelle für `<`, `>` und `|`.
QWERTZ-Bridge ergänzt drei AltGr-Kombinationen, die im deutschen Layout frei sind:

| Tastenkombi (physische Taste) | Ausgabe |
|---|---|
| AltGr + `,` | `<` |
| AltGr + `.` | `>` |
| AltGr + `-` (rechts vom Punkt) | `\|` |

Alle Standard-AltGr-Zeichen (`@ € { } [ ] \ ~`) funktionieren weiterhin.

## Schnellstart

1. `QwertzBridge.exe` aus dem [neuesten Release](https://github.com/dreyssechris/qwertz-bridge/releases/latest) herunterladen
2. Starten. Eine Tray-Benachrichtigung zeigt die aktiven Kombis
3. Fertig. Optional im Tray-Menü *Start with Windows* aktivieren

Weitere Punkte:

- Portabel: eine EXE, die Konfiguration (`qwertzbridge.json`) liegt daneben, keine Adminrechte
- Tray-Menü: Ein/Aus (auch per Doppelklick), Konfiguration öffnen, Autostart, Beenden
- Hot-Reload: Änderungen an der JSON-Datei wirken sofort. Fehlerhafte Configs fallen
  mit Hinweis auf die Defaults zurück
- Profile pro Anwendung über `processNames` in der Konfiguration

Vollständige Dokumentation (Konfiguration, FAQ, Build) im englischen [README.md](README.md).
