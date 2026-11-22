# Pruebas E2E: flujo de órdenes (Postman / Newman)

Automatiza un flujo de caja negra que simula a un cliente usando la API:

1. **Registrar cliente** (paso previo): crea un usuario nuevo en cada ejecución, porque la base de datos no trae usuarios para iniciar sesión.
2. **Login**: obtiene el token JWT y el `userId`, y los guarda solos.
3. **Listar productos**: elige el primer producto activo con stock suficiente y guarda su `id`.
4. **Crear orden**: crea la orden con ese producto y valida la respuesta (201, estado `PENDING`, cliente, producto, cantidad y total).

No hay que copiar ni pegar datos a mano: todo se encadena con variables del environment.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `Dsw2025Tpi.E2E.postman_collection.json` | La colección con las 4 solicitudes y sus scripts de prueba |
| `local.postman_environment.json` | Environment `Dsw2025Tpi - Local` con las variables (sin contraseñas ni tokens) |

## Requisitos

- API levantada en `http://localhost:3000`:
  ```
  cd Back/Dsw2025Tpi.Api
  dotnet run --launch-profile http
  ```
  (necesita .NET 8 y SQL Server LocalDB, sólo Windows).
- Para Newman: Node.js y `npm install -g newman`.

## Ejecutar con Postman

1. Importar los dos archivos (botón **Import**).
2. Elegir el environment **Dsw2025Tpi - Local** (arriba a la derecha).
3. En la colección: **Run** → **Run Dsw2025Tpi - E2E Flujo de ordenes**. Deben pasar todos los tests.

## Ejecutar con Newman (desde la raíz del repo)

```
newman run Back/postman/Dsw2025Tpi.E2E.postman_collection.json -e Back/postman/local.postman_environment.json
```

Con `--bail` se detiene en la primera falla. Newman termina con código 0 si todo pasó y 1 si algo falló.

## Variables

| Variable | Valor | Quién la completa |
|---|---|---|
| `baseUrl` | `http://localhost:3000` | environment |
| `shippingAddress`, `billingAddress`, `quantity` | datos de prueba de la orden | environment |
| `username`, `password`, `email`, `phoneNumber` | usuario de prueba | script del paso 0 (se genera solo) |
| `token`, `userId` | resultado del login | script del paso 1 |
| `productId`, `productName`, `productPrice` | producto elegido | script del paso 2 |
| `orderId` | orden creada | script del paso 3 |

## Notas

- Cada ejecución crea un usuario y una orden nuevos y **descuenta stock** (por lo que se ve en el código, dos veces la cantidad pedida). Con los datos iniciales (50 y 30 unidades) se puede repetir muchas veces, pero no infinitas.
- Rutas usadas: `POST /api/auth/registerCustomer`, `POST /api/auth/login`, `GET /api/products`, `POST /api/orders`.
- El repositorio no guarda contraseñas ni tokens: la clave del usuario de prueba se genera en cada corrida.
