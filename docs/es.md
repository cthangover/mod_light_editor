# Editor de luces — luces fijas de una escena, colocadas sobre una vista nocturna

Las escenas nocturnas de `time_base` se iluminan con la línea `light_set` de un escenario. El editor de luces convierte esa línea en puntos que arrastras sobre el fondo de la escena: la vista previa se dibuja con el propio sombreador nocturno del juego, así que ves cómo quedará la escena al anochecer.

## Cómo abrirlo

Botón del martillo en la barra del juego → **Editor de luces**.

## Colocar luces

![El editor de luces con home_kitchen_cooking.scenario: dos luces en la lista, el fondo de la cocina en la vista nocturna con la luz de una lámpara y el círculo de su radio, y de la luz seleccionada X, Y, Radio 500, Intensidad 1.0 y un color amarillo.](assets/light-editor.jpg)

1. Elige el escenario cuya línea `light_set` editas (● marca los escenarios que ya fijan luces). Se muestra el fondo de su escena; **Fondo de vista previa** elige otro.
2. Doble clic en el fondo (o **+ Luz**) añade una luz, arrastrar las mueve, Supr borra la seleccionada. Como máximo 10 luces fijas.
3. **Luz seleccionada**: **X**/**Y** (0–1 de la pantalla), **Radio** (píxeles de la pantalla de juego de 1920 px), **Intensidad** (0–1) y **Color**.
4. Ctrl+S escribe la línea `light_set` en el escenario; si el escenario no la tiene, se inserta tras su cabecera. **Copiar línea** en cambio copia el comando al portapapeles para pegarlo a mano.

## Límites

- Solo mods de carpeta; los escenarios empaquetados no se pueden escribir (la línea sí se puede copiar).
- Las luces siguen al escenario: otro escenario de la misma escena puede fijar otras.

## Dependencias

`tools_base`, `time_base`, `ui_base`, `scenario_tools_base` (lee escenarios; no hace falta la ventana del editor de escenarios).
El paquete antiguo `editor_tools` instala este mod junto con las demás ventanas de autor.
