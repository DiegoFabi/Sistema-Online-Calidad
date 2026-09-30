using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaOnline.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cartas",
                columns: table => new
                {
                    ID_Carta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Carta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cantidad_Platos = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cartas", x => x.ID_Carta);
                });

            migrationBuilder.CreateTable(
                name: "Categorias_Ingredientes",
                columns: table => new
                {
                    ID_Cat_Ingrediente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias_Ingredientes", x => x.ID_Cat_Ingrediente);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    ID_Mesa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero_Mesa = table.Column<int>(type: "int", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.ID_Mesa);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    ID_Promocion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Porcentaje_Descuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Fecha_Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.ID_Promocion);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    ID_Proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Empresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Email_Contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Tipo_Suministro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.ID_Proveedor);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID_Rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID_Rol);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    ID_Turno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Turno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Hora_Inicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    Hora_Fin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Dias_Semana = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.ID_Turno);
                });

            migrationBuilder.CreateTable(
                name: "Productos_Categorias",
                columns: table => new
                {
                    ID_Categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ID_Carta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos_Categorias", x => x.ID_Categoria);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_Cartas_ID_Carta",
                        column: x => x.ID_Carta,
                        principalTable: "Cartas",
                        principalColumn: "ID_Carta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredientes",
                columns: table => new
                {
                    ID_Ingrediente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Ingrediente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unidad_Medida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Costo_Unitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    ID_Cat_Ingrediente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredientes", x => x.ID_Ingrediente);
                    table.ForeignKey(
                        name: "FK_Ingredientes_Categorias_Ingredientes_ID_Cat_Ingrediente",
                        column: x => x.ID_Cat_Ingrediente,
                        principalTable: "Categorias_Ingredientes",
                        principalColumn: "ID_Cat_Ingrediente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos_Proveedores",
                columns: table => new
                {
                    ID_Contrato_Proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo_Contrato = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Clausula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ID_Proveedor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos_Proveedores", x => x.ID_Contrato_Proveedor);
                    table.ForeignKey(
                        name: "FK_Contratos_Proveedores_Proveedores_ID_Proveedor",
                        column: x => x.ID_Proveedor,
                        principalTable: "Proveedores",
                        principalColumn: "ID_Proveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    ID_Usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    ID_Rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.ID_Usuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_ID_Rol",
                        column: x => x.ID_Rol,
                        principalTable: "Roles",
                        principalColumn: "ID_Rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ID_Producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Plato = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tiempo_Preparacion = table.Column<double>(type: "float", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Disponibilidad = table.Column<bool>(type: "bit", nullable: false),
                    ID_Categoria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ID_Producto);
                    table.ForeignKey(
                        name: "FK_Productos_Productos_Categorias_ID_Categoria",
                        column: x => x.ID_Categoria,
                        principalTable: "Productos_Categorias",
                        principalColumn: "ID_Categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    ID_Inventario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad_Stock = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fecha_Ultima_Reposicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stock_Minimo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stock_Maximo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_Ingrediente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.ID_Inventario);
                    table.ForeignKey(
                        name: "FK_Inventarios_Ingredientes_ID_Ingrediente",
                        column: x => x.ID_Ingrediente,
                        principalTable: "Ingredientes",
                        principalColumn: "ID_Ingrediente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores_Ingredientes",
                columns: table => new
                {
                    ID_Proveedor = table.Column<int>(type: "int", nullable: false),
                    ID_Ingrediente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores_Ingredientes", x => new { x.ID_Proveedor, x.ID_Ingrediente });
                    table.ForeignKey(
                        name: "FK_Proveedores_Ingredientes_Ingredientes_ID_Ingrediente",
                        column: x => x.ID_Ingrediente,
                        principalTable: "Ingredientes",
                        principalColumn: "ID_Ingrediente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Proveedores_Ingredientes_Proveedores_ID_Proveedor",
                        column: x => x.ID_Proveedor,
                        principalTable: "Proveedores",
                        principalColumn: "ID_Proveedor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ID_Cliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Apellidos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fecha_Nacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DNI = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    RUC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    ID_Usuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ID_Cliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_ID_Usuario",
                        column: x => x.ID_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "ID_Usuario");
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    ID_Empleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ID_Usuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.ID_Empleado);
                    table.ForeignKey(
                        name: "FK_Empleados_Usuarios_ID_Usuario",
                        column: x => x.ID_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "ID_Usuario");
                });

            migrationBuilder.CreateTable(
                name: "Productos_Ingredientes",
                columns: table => new
                {
                    ID_Producto_Ingrediente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    Unidad_Medida = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ID_Ingrediente = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos_Ingredientes", x => x.ID_Producto_Ingrediente);
                    table.ForeignKey(
                        name: "FK_Productos_Ingredientes_Ingredientes_ID_Ingrediente",
                        column: x => x.ID_Ingrediente,
                        principalTable: "Ingredientes",
                        principalColumn: "ID_Ingrediente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_Ingredientes_Productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "Productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productos_Promociones",
                columns: table => new
                {
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    ID_Promocion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos_Promociones", x => new { x.ID_Producto, x.ID_Promocion });
                    table.ForeignKey(
                        name: "FK_Productos_Promociones_Productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "Productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_Promociones_Promociones_ID_Promocion",
                        column: x => x.ID_Promocion,
                        principalTable: "Promociones",
                        principalColumn: "ID_Promocion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservaciones",
                columns: table => new
                {
                    ID_Reservacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Hora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Numero_Personas = table.Column<int>(type: "int", nullable: false),
                    Ocasion_Especial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado_Reservacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ID_Cliente = table.Column<int>(type: "int", nullable: false),
                    ID_Mesa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservaciones", x => x.ID_Reservacion);
                    table.ForeignKey(
                        name: "FK_Reservaciones_Clientes_ID_Cliente",
                        column: x => x.ID_Cliente,
                        principalTable: "Clientes",
                        principalColumn: "ID_Cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservaciones_Mesas_ID_Mesa",
                        column: x => x.ID_Mesa,
                        principalTable: "Mesas",
                        principalColumn: "ID_Mesa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos_Empleados",
                columns: table => new
                {
                    ID_Contrato_Empleado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_Fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo_Contrato = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Salario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Clausula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ID_Empleado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos_Empleados", x => x.ID_Contrato_Empleado);
                    table.ForeignKey(
                        name: "FK_Contratos_Empleados_Empleados_ID_Empleado",
                        column: x => x.ID_Empleado,
                        principalTable: "Empleados",
                        principalColumn: "ID_Empleado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Empleados_Turnos",
                columns: table => new
                {
                    ID_Turno = table.Column<int>(type: "int", nullable: false),
                    ID_Empleado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados_Turnos", x => new { x.ID_Turno, x.ID_Empleado });
                    table.ForeignKey(
                        name: "FK_Empleados_Turnos_Empleados_ID_Empleado",
                        column: x => x.ID_Empleado,
                        principalTable: "Empleados",
                        principalColumn: "ID_Empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Empleados_Turnos_Turnos_ID_Turno",
                        column: x => x.ID_Turno,
                        principalTable: "Turnos",
                        principalColumn: "ID_Turno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    ID_Pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado_Pedido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_Empleado = table.Column<int>(type: "int", nullable: false),
                    ID_Cliente = table.Column<int>(type: "int", nullable: true),
                    ID_Mesa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.ID_Pedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_ID_Cliente",
                        column: x => x.ID_Cliente,
                        principalTable: "Clientes",
                        principalColumn: "ID_Cliente");
                    table.ForeignKey(
                        name: "FK_Pedidos_Empleados_ID_Empleado",
                        column: x => x.ID_Empleado,
                        principalTable: "Empleados",
                        principalColumn: "ID_Empleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_Mesas_ID_Mesa",
                        column: x => x.ID_Mesa,
                        principalTable: "Mesas",
                        principalColumn: "ID_Mesa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comprobantes_Pagos",
                columns: table => new
                {
                    ID_Comprobante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo_Comprobante = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Numero_Comprobante = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Fecha_Emision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sub_Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Monto_Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IGV = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado_Comprobante = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Metodo_Pago = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Razon_Social = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RUC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Direccion_Fiscal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comprobantes_Pagos", x => x.ID_Comprobante);
                    table.ForeignKey(
                        name: "FK_Comprobantes_Pagos_Pedidos_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "Pedidos",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    ID_Pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha_Hora_Pago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Metodo_Pago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Detalles_Tarjeta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.ID_Pago);
                    table.ForeignKey(
                        name: "FK_Pagos_Pedidos_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "Pedidos",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos_Detalles",
                columns: table => new
                {
                    ID_Pedido_Detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos_Detalles", x => x.ID_Pedido_Detalle);
                    table.ForeignKey(
                        name: "FK_Pedidos_Detalles_Pedidos_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "Pedidos",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedidos_Detalles_Productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "Productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ID_Usuario",
                table: "Clientes",
                column: "ID_Usuario",
                unique: true,
                filter: "[ID_Usuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_Pagos_ID_Pedido",
                table: "Comprobantes_Pagos",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Empleados_ID_Empleado",
                table: "Contratos_Empleados",
                column: "ID_Empleado");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Proveedores_ID_Proveedor",
                table: "Contratos_Proveedores",
                column: "ID_Proveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_ID_Usuario",
                table: "Empleados",
                column: "ID_Usuario",
                unique: true,
                filter: "[ID_Usuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Turnos_ID_Empleado",
                table: "Empleados_Turnos",
                column: "ID_Empleado");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_ID_Cat_Ingrediente",
                table: "Ingredientes",
                column: "ID_Cat_Ingrediente");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ID_Ingrediente",
                table: "Inventarios",
                column: "ID_Ingrediente");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ID_Pedido",
                table: "Pagos",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Cliente",
                table: "Pedidos",
                column: "ID_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Empleado",
                table: "Pedidos",
                column: "ID_Empleado");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ID_Mesa",
                table: "Pedidos",
                column: "ID_Mesa");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Detalles_ID_Pedido",
                table: "Pedidos_Detalles",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Detalles_ID_Producto",
                table: "Pedidos_Detalles",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ID_Categoria",
                table: "Productos",
                column: "ID_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Categorias_ID_Carta",
                table: "Productos_Categorias",
                column: "ID_Carta");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Ingredientes_ID_Ingrediente",
                table: "Productos_Ingredientes",
                column: "ID_Ingrediente");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Ingredientes_ID_Producto",
                table: "Productos_Ingredientes",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Promociones_ID_Promocion",
                table: "Productos_Promociones",
                column: "ID_Promocion");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Ingredientes_ID_Ingrediente",
                table: "Proveedores_Ingredientes",
                column: "ID_Ingrediente");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaciones_ID_Cliente",
                table: "Reservaciones",
                column: "ID_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Reservaciones_ID_Mesa",
                table: "Reservaciones",
                column: "ID_Mesa");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ID_Rol",
                table: "Usuarios",
                column: "ID_Rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comprobantes_Pagos");

            migrationBuilder.DropTable(
                name: "Contratos_Empleados");

            migrationBuilder.DropTable(
                name: "Contratos_Proveedores");

            migrationBuilder.DropTable(
                name: "Empleados_Turnos");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Pedidos_Detalles");

            migrationBuilder.DropTable(
                name: "Productos_Ingredientes");

            migrationBuilder.DropTable(
                name: "Productos_Promociones");

            migrationBuilder.DropTable(
                name: "Proveedores_Ingredientes");

            migrationBuilder.DropTable(
                name: "Reservaciones");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Ingredientes");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropTable(
                name: "Productos_Categorias");

            migrationBuilder.DropTable(
                name: "Categorias_Ingredientes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Cartas");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
