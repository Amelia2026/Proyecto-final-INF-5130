# Declaración de uso de IA · INF-5130 TF02

**Autora:** Amelia A. Lopez Ramirez 100735423

## Herramienta

Claude (Anthropic), usado mediante su chat.

## Aporte de la herramienta

- **Generación de código:** Con ayuda de Claude, parte de Domain, Application y la API, y me ayudo a completar parte de las pruebas xunit.
- **Explicación de conceptos y arquitectura:** capas, estrategias de tarifa, JWT y roles, migraciones de EF Core.
- **Revisión y depuración de errores:**  bloqueo de los DLL al recompilar con la API en ejecución, y la configuración de puertos y del certificado de desarrollo.
- **Guía en la auditoría final:** revisión del proyecto contra la Guía facilitada, el contrato de la API y los criterios de aceptación del Maestro, apoyo para ordenar las evidencias y los commits.

## Verificaciones realizadas

La IA no se tomó como fuente de verdad. Todo se comprobó ejecutándolo en mi equipo:

- Recompilación de la solución: 8 proyectos correctos, 0 errores.
- 37 pruebas xUnit superadas (25 de Domain, 10 de Application y 2 de Infrastructure).
- Recorrido automático de la API con `probar-api.ps1`: 21 pasos con el código HTTP esperado, incluidos 401 sin token, 403 para el Gestor, rechazo de reservas solapadas y rechazo del check-out repetido.
- Prueba del frontend del profesor conectado a mi API y a SQL Server: las tarifas mostradas (Sencilla RD$3,000, Doble RD$3,800) coinciden con la base de datos.
- Revisión de las bases de datos directamente en DBeaver.
- Lectura del código y comparación con el contrato y los criterios del Maestro.

## Responsabilidad

Soy responsable del código que entrego y puedo explicarlo en la defensa.
