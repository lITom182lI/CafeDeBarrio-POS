# Guía de Configuración — Café de Barrio POS

## Requisitos previos
- Windows 10/11 64-bit
- Acceso por red local al servidor donde corre el API

## Configurar la conexión al servidor

1. Abre la carpeta de instalación
   (por defecto: C:\Program Files\CafeDeBarrio\)
2. Edita el archivo App.config con el Bloc de notas
3. Cambia el valor de ApiBaseUrl:

     <add key="ApiBaseUrl" value="http://IP_DEL_SERVIDOR:5138" />

   Ejemplo: http://192.168.1.10:5138

4. Guarda el archivo y abre la aplicación

## Credenciales iniciales
- Usuario:    admin@cafedebarrio.com
- Contraseña: Admin2026!
Cambiar la contraseña en el primer ingreso.

## Backup automático
El backup corre todos los días a las 23:00.
Archivos guardados en: C:\Users\[usuario]\CafeDeBarrio\backups\
Se conservan los últimos 7 backups automáticamente.
