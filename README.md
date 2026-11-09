# Isaac Save Manager

Programa portable para Windows que gestiona los guardados de progreso de **The Binding of Isaac: Repentance** y **Repentance+**. Sirve para:

- poner en una ranura del juego un guardado que tengas suelto (una partida 100 %, una partida nueva para speedrun, una copia antigua…);
- pasar el progreso de **Repentance a Repentance+** o al revés;
- copiar una ranura a otra, exportarla y restaurar copias de seguridad.

No es un mod y no modifica el juego. Es un solo `.exe` que no necesita instalación (usa .NET Framework 4.8, que ya viene con Windows 10 y 11).

> ☕ **¿Te sirve?** Puedes apoyar el proyecto en **[Ko-fi](https://ko-fi.com/maflatv)**.
> Sígueme en [YouTube](https://www.youtube.com/@MaflaTV) · [Twitch](https://www.twitch.tv/maflatv) · [TikTok](https://www.tiktok.com/@maflatv) · [Instagram](https://www.instagram.com/mafla.tv/) · [GitHub](https://github.com/maflatv).
> Estos enlaces también están en la aplicación: en la barra inferior, en el menú *Ayuda* (*Apoyar en Ko-fi*, *Sígueme*, *Acerca de*) y en la ventana de preguntas frecuentes.

**Descarga:** el `.exe` listo para usar está en [Releases](https://github.com/maflatv/Isaac-Save-Manager/releases).

## Steam Cloud: apagar antes, encender antes de jugar

Si juegas en Steam con Steam Cloud, cambiar el archivo no basta, porque Steam volvería a poner el guardado de la nube. El procedimiento es este:

1. **Apaga Steam Cloud para Isaac** a mano en Steam: en la Biblioteca, abre *The Binding of Isaac: Rebirth* → engranaje ⚙ de la derecha → *Propiedades…* → *General* → interruptor del recuadro *Steam Cloud*. Después **sal de Steam del todo** (icono junto al reloj → *Salir de Steam*).
2. **Gestiona los archivos** con este programa.
3. **Vuelve a encender Steam Cloud antes de jugar**, para que Steam detecte el archivo nuevo y lo suba. Si Steam avisa de un conflicto, elige **la copia de este equipo**: la de la nube es el guardado antiguo.

El programa no toca ese ajuste de Steam; solo lo lee (`userdata\<cuenta>\7\remote\sharedconfig.vdf`). Mientras esté encendido, no escribe en las ubicaciones de Steam.

Ayudas que incluye:
- **Barra de 3 fases** en la ventana principal: *Apagar Steam Cloud → Gestionar archivos → Encender Steam Cloud y jugar*. Marca la fase actual según el estado detectado, y al pulsar una fase se abre su explicación.
- **Guía ilustrada de 6 pasos** (botón *Instrucciones de Steam Cloud* o *Ayuda → Guía de Steam Cloud*), dibujada a partir de la interfaz actual de Steam:
  1. Engranaje de la página del juego → *Propiedades…*
  2. Apagar el interruptor de Steam Cloud.
  3. Salir de Steam.
  4. Gestionar los archivos aquí.
  5. Volver a encender Steam Cloud.
  6. Qué elegir si aparece un conflicto.
- Después de cada reemplazo en Steam, la guía se abre sola en el paso 5.

## Uso

1. **Cierra el juego** y, si usas Steam Cloud, sigue los pasos de la sección anterior.
2. Abre `IsaacSaveManager.exe` y elige la ubicación:
   - **Steam Cloud: <cuenta>**: lo que usa el juego si Steam Cloud está activado en sus opciones (lo normal en Steam). Aparece una entrada por cada cuenta de Steam del equipo.
   - **Documentos**: lo que usa el juego si Steam Cloud está desactivado o no hay Steam.
   - **Otra carpeta…**: cualquier carpeta con archivos `rep+persistentgamedataN.dat` / `rep_persistentgamedataN.dat`.
3. Selecciona la ranura y elige una acción:
   - **Reemplazar con archivo…** (o doble clic en la ranura): elige un `.dat`. Si es de la otra versión, se convierte solo.
   - **Copiar a otra ranura**: por ejemplo, de *Repentance, ranura 1* a *Repentance+, ranura 1* para pasar el progreso a Repentance+.
   - **Exportar…**: guarda una copia. El tipo de archivo del diálogo elige la versión (Repentance+ o Repentance).
   - **Restaurar copia…**: vuelve a poner una copia de seguridad de esa ranura.
   - **Copia de seguridad ahora**: copia de una vez las 6 ranuras de la ubicación elegida, sin modificar nada. No hace falta cerrar el juego ni apagar Steam Cloud.
   - **Convertir archivo suelto…**: convierte un `.dat` a la otra versión sin tocar el juego.
4. Abre Steam, **vuelve a encender Steam Cloud para Isaac** y luego inicia el juego. Si Steam avisa de un conflicto, elige **la copia de este equipo**.

Antes de escribir, el programa siempre copia el archivo actual a la carpeta `backups` junto al `.exe` (o a `%LOCALAPPDATA%\IsaacSaveManager\backups` si esa carpeta no admite escritura).

## Ayuda dentro del programa

El menú **Ayuda** tiene:
- **Preguntas frecuentes** (tecla **F1**): 10 preguntas con su respuesta, en el idioma elegido.
- **Guía de Steam Cloud**: la guía ilustrada de 6 pasos.
- **Apoyar en Ko-fi** y **Acerca de**: versión, autor, enlace de Ko-fi y créditos.

## Preguntas frecuentes

**¿Dónde están mis guardados?**
En la carpeta de Steam de cada cuenta (si la opción Steam Cloud del juego está activada, lo normal) o en `Documentos\My Games\Binding of Isaac Repentance(+)`. El texto gris bajo el desplegable *Ubicación* indica cuál usa el juego.

**¿Cómo pongo en el juego un guardado que descargué?**
Apaga Steam Cloud y sal de Steam. Selecciona la ranura de destino, pulsa *Reemplazar con archivo…* y elige el `.dat`. Antes de jugar, vuelve a encender Steam Cloud.

**¿Cómo paso mi progreso de Repentance a Repentance+?**
Selecciona la ranura de Repentance, pulsa *Copiar a otra ranura* y elige una de Repentance+. Se convierte solo y no se pierde nada. Al revés también funciona, pero se pierden los datos exclusivos de Repentance+, y el programa avisa antes.

**¿Qué significan los estados de la lista?**
- *Correcta*: guardado válido.
- *Vacía*: no hay archivo.
- *Contiene un guardado de…*: el archivo es de la otra versión (por ejemplo, renombrado a mano).
- *En rojo*: el archivo está dañado; el texto dice por qué.

**¿Cómo deshago un cambio?**
Selecciona la ranura y pulsa *Restaurar copia…*. Cada escritura deja antes una copia con fecha y hora en `backups`.

**¿Qué no hace?**
- No toca la partida en curso (`gamestate`).
- No usa guardados de Rebirth, Afterbirth ni Afterbirth+.
- No cambia ajustes de Steam.
- No edita logros sueltos.

**Errores comunes**
- *El juego está abierto*: ciérralo.
- *Steam Cloud está activado*: apágalo, sal de Steam y pulsa *Actualizar*.
- *Checksum no coincide*: el archivo está dañado; consigue otra copia.
- *Sin permiso*: ejecuta como administrador.

## Idiomas

Español, English, Français, Português y Русский. Por defecto se usa el idioma del sistema (inglés si no es ninguno de esos), y se puede elegir otro en el selector *Idioma*. La elección se guarda en `IsaacSaveManager.ini` junto al `.exe`. Los textos están en `src/Strings.<idioma>.cs`.

## Repentance frente a Repentance+

Los archivos no se diferencian solo en el nombre. Tienen el mismo formato, pero Repentance+ guarda más datos:

| | Repentance (`rep_`) | Repentance+ (`rep+`) |
|---|---|---|
| Logros | 637 (+1 vacía) | 641 (+1 vacía) |
| Contadores de eventos | 496 | 523 |

- **Repentance → Repentance+**: no se pierde nada. Las entradas nuevas quedan a cero, igual que cuando un parche del juego añade logros.
- **Repentance+ → Repentance**: se pierden los 4 logros y los contadores exclusivos de Repentance+. El programa avisa antes con el número exacto.

Si alguien renombró a mano un `rep_` como `rep+`, la columna *Estado* lo indica («Contiene un guardado de Repentance»). Reemplazar la ranura con ese mismo archivo lo convierte.

Solo se usan los archivos de progreso (`persistentgamedata`). Las partidas en curso (`gamestate`) no se tocan. Los guardados de Rebirth, Afterbirth y Afterbirth+ se rechazan.

## Formato del archivo (para desarrolladores)

- `0x00` cabecera `ISAACNGSAVE09R  ` · `0x10` u32 opaco · desde `0x14`, bloques 1 a 11 · 4 bytes opacos · checksum.
- Cada bloque tiene una cabecera `(tipo, tamaño, cantidad)` seguida de los datos. El campo *tamaño* no es fiable; se usa *cantidad* × tamaño del elemento:
  - 1 byte por elemento: tipos 1, 4, 5, 6, 7 y 10.
  - 4 bytes por elemento: tipos 2, 3, 8 y 9.
- El bloque 11 es el bestiario: *cantidad* sub-bloques, cada uno con la cabecera `(id, n×4)` y `n` entradas de 8 bytes.
- El checksum (los últimos 4 bytes) es un CRC de `[0x10, fin−4)`:
  - Valor inicial `~0xFEDCBA76`, XOR final `0xFFFFFFFF`.
  - Usa una tabla propia del juego, distinta de la del CRC‑32 estándar.
  - El nombre del archivo no entra en el cálculo.

## Compilar

`build.bat` genera `dist\IsaacSaveManager.exe` y `build\SelfTest.exe`. Necesita el compilador C# de Visual Studio o de Build Tools. `SelfTest.exe <carpetas o archivos>` comprueba el núcleo con guardados reales (solo los lee) y prueba la escritura en una carpeta temporal.

El icono es `src\app.ico`, incrustado en el `.exe` y también como recurso para la ventana *Acerca de*. Para cambiarlo por otro sprite, ejecuta `powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1 <sprite.png>` y vuelve a compilar. El script recorta el sprite, lo amplía sin suavizar y genera tamaños de 16 a 256 px.

## Créditos

Hecho por **MaflaTV** · ☕ [Ko-fi](https://ko-fi.com/maflatv) · [YouTube](https://www.youtube.com/@MaflaTV) · [Twitch](https://www.twitch.tv/maflatv) · [TikTok](https://www.tiktok.com/@maflatv) · [Instagram](https://www.instagram.com/mafla.tv/) · [GitHub](https://github.com/maflatv)

La tabla del checksum procede de [isaac-save-edit-script](https://github.com/jamesthejellyfish/isaac-save-edit-script) (licencia MIT), basado a su vez en el *Afterbirth Save Editor*. El icono usa el sprite de *Moving Box*, un objeto del juego. *The Binding of Isaac* y su arte son de Edmund McMillen y Nicalis. Este programa no está afiliado a ellos.

## Licencia

El código es [MIT](LICENSE). Los avisos de terceros (la licencia de la tabla del checksum y el origen del icono, que no está cubierto por la licencia MIT) están en [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

---

## English

Portable Windows tool for **The Binding of Isaac: Repentance / Repentance+** progress saves.

- Drop a loose save into a game slot.
- Convert progress between Repentance and Repentance+.
- Copy, export and restore slots, with automatic backups.

**Steam Cloud:**
1. Turn it off for Isaac by hand in Steam (game page → gear ⚙ → Properties → General) and fully exit Steam before changing files.
2. Manage the files with this program.
3. Turn it back on before playing, so Steam picks up the new file. On a conflict, keep the copy on this computer.

The tool reads that setting and will not write to Steam folders while it is on. A 3-phase bar and a 6-step illustrated guide are built in.

**Help:** press F1 or open *Help → Frequently asked questions* for 10 answered questions about how the program works.

**Languages:** Spanish, English, French, Portuguese and Russian. The interface follows the system language by default.

**Rep vs Rep+:** the two versions differ in content, not just in file name. Repentance+ has 4 more achievements and 27 more event counters. Going Rep → Rep+ is lossless. Going Rep+ → Rep drops that extra data, and the tool warns you first.

**Download:** get the ready-to-use `.exe` from [Releases](https://github.com/maflatv/Isaac-Save-Manager/releases).

☕ **Find it useful?** Support the project on **[Ko-fi](https://ko-fi.com/maflatv)**. Made by MaflaTV. Follow me on [YouTube](https://www.youtube.com/@MaflaTV) · [Twitch](https://www.twitch.tv/maflatv) · [TikTok](https://www.tiktok.com/@maflatv) · [Instagram](https://www.instagram.com/mafla.tv/) · [GitHub](https://github.com/maflatv).
