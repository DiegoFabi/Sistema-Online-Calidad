/* =====================================================================
   Panel de accesibilidad propio de El Rancho Sagrado.
   Reemplaza al widget de UserWay. Cubre las cuatro consideraciones de
   accesibilidad del proyecto:
     1. Daltonismo            -> filtros de color, enlaces sin depender del color
     2. Baja visión           -> tamaño de texto, alto contraste, espaciado
     3. Dislexia y lectura    -> fuente legible, guía de lectura, sin animaciones
     4. Movilidad y teclado   -> foco resaltado, cursor grande
   Las preferencias se guardan en localStorage y se aplican en cada página.
   ===================================================================== */
(function () {
    'use strict';

    var CLAVE = 'rs-a11y';
    var TAM_MAX = 4;
    var html = document.documentElement;

    // Matrices de corrección (daltonización) calculadas a partir de las simulaciones
    // de Machado, Oliveira y Fernandes (2009): el error de color que la persona no
    // percibe se traslada a los canales que sí distingue.
    var MATRICES = {
        protanopia: '1 0 0 0 0  0.479 0.477 0.044 0 0  0.597 -0.689 1.091 0 0  0 0 0 1 0',
        deuteranopia: '1 0 0 0 0  0.163 0.725 0.112 0 0  0.455 -0.645 1.191 0 0  0 0 0 1 0',
        tritanopia: '0.741 -0.407 0.666 0 0  0.075 0.585 0.340 0 0  0 0 1 0 0  0 0 0 1 0'
    };

    var BASE = {
        color: 'ninguno', tam: 0, contraste: false, espaciado: false, enlaces: false,
        fuente: false, guia: false, animacion: false, foco: false, cursor: false
    };

    function leer() {
        try {
            var g = JSON.parse(localStorage.getItem(CLAVE) || '{}');
            var p = {};
            Object.keys(BASE).forEach(function (k) { p[k] = (k in g) ? g[k] : BASE[k]; });
            return p;
        } catch (e) { return Object.assign({}, BASE); }
    }

    function guardar(p) {
        try { localStorage.setItem(CLAVE, JSON.stringify(p)); } catch (e) { /* modo privado */ }
    }

    var pref = leer();

    // ------------------------------------------------------------ aplicar
    function asegurarFiltros() {
        if (document.getElementById('rs-a11y-filtros')) return;
        var ns = 'http://www.w3.org/2000/svg';
        var svg = document.createElementNS(ns, 'svg');
        svg.setAttribute('id', 'rs-a11y-filtros');
        svg.setAttribute('aria-hidden', 'true');
        svg.setAttribute('focusable', 'false');
        svg.style.cssText = 'position:absolute;width:0;height:0;overflow:hidden';
        Object.keys(MATRICES).forEach(function (k) {
            var f = document.createElementNS(ns, 'filter');
            f.setAttribute('id', 'rs-f-' + k);
            f.setAttribute('color-interpolation-filters', 'linearRGB');
            var m = document.createElementNS(ns, 'feColorMatrix');
            m.setAttribute('type', 'matrix');
            m.setAttribute('values', MATRICES[k]);
            f.appendChild(m);
            svg.appendChild(f);
        });
        document.body.appendChild(svg);
    }

    function aplicar() {
        for (var i = 1; i <= TAM_MAX; i++) html.classList.toggle('rs-tam-' + i, pref.tam === i);
        html.classList.toggle('rs-espaciado', pref.espaciado);
        html.classList.toggle('rs-enlaces', pref.enlaces);
        html.classList.toggle('rs-fuente', pref.fuente);
        html.classList.toggle('rs-sin-animacion', pref.animacion);
        html.classList.toggle('rs-foco', pref.foco);
        html.classList.toggle('rs-cursor', pref.cursor);

        // El filtro se aplica al elemento raíz: así no altera la posición de los
        // elementos fijos (barras laterales, modales, este mismo botón).
        var filtros = [];
        if (MATRICES[pref.color]) { asegurarFiltros(); filtros.push('url(#rs-f-' + pref.color + ')'); }
        if (pref.color === 'grises') filtros.push('grayscale(1)');
        if (pref.contraste) filtros.push('contrast(1.35)');
        html.style.filter = filtros.join(' ');

        var guia = document.getElementById('rs-a11y-guia');
        if (guia) guia.hidden = !pref.guia;

        var punto = document.querySelector('#rs-a11y-btn .rs-a11y-punto');
        if (punto) punto.hidden = !hayCambios();
    }

    function hayCambios() {
        return Object.keys(BASE).some(function (k) { return pref[k] !== BASE[k]; });
    }

    // Se aplica de inmediato (el script carga en <head>) para evitar parpadeos.
    for (var t = 1; t <= TAM_MAX; t++) html.classList.toggle('rs-tam-' + t, pref.tam === t);

    // ------------------------------------------------------------ iconos
    function icono(d) {
        return '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false" fill="currentColor"><path d="' + d + '"/></svg>';
    }

    var I = {
        persona: 'M12 2a2 2 0 1 1 0 4 2 2 0 0 1 0-4zm9 7h-6v13h-2v-6h-2v6H9V9H3V7h18v2z',
        paleta: 'M12 3a9 9 0 0 0 0 18c.8 0 1.5-.7 1.5-1.5 0-.4-.1-.7-.4-1-.2-.3-.4-.6-.4-1 0-.8.7-1.5 1.5-1.5H16a5 5 0 0 0 5-5c0-4.4-4-8-9-8zM6.5 12a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3zm3-4a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3zm5 0a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3zm3 4a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3z',
        contraste: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm1 17.9V4.1a8 8 0 0 1 0 15.8z',
        grises: 'M12 3l-7 9a7 7 0 1 0 14 0l-7-9zm0 16a5 5 0 0 1-5-5h10a5 5 0 0 1-5 5z',
        enlace: 'M10.6 13.4a1 1 0 0 1 0-1.4l3.5-3.5a1 1 0 1 1 1.4 1.4L12 13.4a1 1 0 0 1-1.4 0zM8 18a4 4 0 0 1-2.8-6.8l2.1-2.1 1.4 1.4-2.1 2.1A2 2 0 1 0 9.4 15.4l2.1-2.1 1.4 1.4-2.1 2.1A4 4 0 0 1 8 18zm8.7-3.1-1.4-1.4 2.1-2.1a2 2 0 1 0-2.8-2.8l-2.1 2.1-1.4-1.4 2.1-2.1a4 4 0 1 1 5.6 5.6l-2.1 2.1zM4 20h16v2H4z',
        espaciado: 'M3 5h18v2H3zm0 6h18v2H3zm0 6h18v2H3zM1 3v18H0V3zm23 0v18h-1V3z',
        fuente: 'M9.9 4h4.2L20 20h-2.9l-1.4-4H8.3l-1.4 4H4L9.9 4zm-.7 9.5h5.6L12 6.2 9.2 13.5z',
        guia: 'M2 10h20v4H2zm0-6h20v2H2zm0 14h20v2H2z',
        pausa: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zM10 16H8V8h2v8zm6 0h-2V8h2v8z',
        foco: 'M3 3h6v2H5v4H3V3zm12 0h6v6h-2V5h-4V3zM3 15h2v4h4v2H3v-6zm16 0h2v6h-6v-2h4v-4zM8 8h8v8H8z',
        cursor: 'M5 2l14 12h-6.5l3.8 7-2.6 1.3-3.8-7.1L5 20z'
    };

    // ------------------------------------------------------------ interfaz
    function op(id, texto, ic, extra) {
        return '<button type="button" class="rs-a11y-op" data-op="' + id + '"' + (extra || '') +
            ' aria-pressed="false">' + icono(ic) + '<span>' + texto + '</span></button>';
    }

    function seccion(titulo, ayuda, contenido) {
        return '<section class="rs-a11y-sec" aria-label="' + titulo + '"><h3><span aria-hidden="true"></span>' + titulo +
            '</h3><p>' + ayuda + '</p>' + contenido + '</section>';
    }

    function construir() {
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.id = 'rs-a11y-btn';
        btn.setAttribute('aria-label', 'Abrir el menú de accesibilidad');
        btn.setAttribute('aria-controls', 'rs-a11y-panel');
        btn.setAttribute('aria-expanded', 'false');
        btn.innerHTML = icono(I.persona) + '<span class="rs-a11y-punto" hidden></span>';

        var capa = document.createElement('div');
        capa.id = 'rs-a11y-capa';
        capa.hidden = true;

        var panel = document.createElement('div');
        panel.id = 'rs-a11y-panel';
        panel.hidden = true;
        panel.setAttribute('role', 'dialog');
        panel.setAttribute('aria-modal', 'true');
        panel.setAttribute('aria-labelledby', 'rs-a11y-titulo');
        panel.innerHTML =
            '<div class="rs-a11y-cab">' + icono(I.persona) +
            '<div><h2 id="rs-a11y-titulo">Accesibilidad</h2><p>Ajusta el sistema a tu forma de ver y navegar</p></div>' +
            '<button type="button" class="rs-a11y-cerrar" aria-label="Cerrar el menú de accesibilidad">&times;</button></div>' +
            '<div class="rs-a11y-cuerpo">' +
            seccion('Visión del color', 'Filtros para daltonismo y señales que no dependen del color.',
                '<div class="rs-a11y-grid" role="group" aria-label="Filtro de color">' +
                op('color:protanopia', 'Protanopía', I.paleta) +
                op('color:deuteranopia', 'Deuteranopía', I.paleta) +
                op('color:tritanopia', 'Tritanopía', I.paleta) +
                op('color:grises', 'Escala de grises', I.grises) +
                '</div><div class="rs-a11y-grid" style="margin-top:8px">' +
                op('enlaces', 'Resaltar enlaces y botones', I.enlace) + '</div>') +
            seccion('Baja visión', 'Agranda el texto, refuerza el contraste o separa las líneas.',
                '<div class="rs-a11y-tam"><button type="button" data-tam="-1" aria-label="Reducir el tamaño del texto">A-</button>' +
                '<output id="rs-a11y-tam-valor" aria-live="polite">100 %</output>' +
                '<button type="button" data-tam="1" aria-label="Aumentar el tamaño del texto">A+</button></div>' +
                '<div class="rs-a11y-barra" aria-hidden="true"><i></i></div>' +
                '<div class="rs-a11y-grid">' + op('contraste', 'Alto contraste', I.contraste) +
                op('espaciado', 'Espaciado de texto', I.espaciado) + '</div>') +
            seccion('Lectura y dislexia', 'Facilita seguir el texto y reduce las distracciones.',
                '<div class="rs-a11y-grid">' + op('fuente', 'Fuente legible', I.fuente) +
                op('guia', 'Guía de lectura', I.guia) + op('animacion', 'Detener animaciones', I.pausa) + '</div>') +
            seccion('Movilidad y teclado', 'Ubica mejor el foco y el puntero al navegar.',
                '<div class="rs-a11y-grid">' + op('foco', 'Resaltar foco', I.foco) +
                op('cursor', 'Cursor grande', I.cursor) + '</div>') +
            '</div>' +
            '<div class="rs-a11y-pie"><button type="button" class="rs-a11y-restablecer">Restablecer</button>' +
            '<button type="button" class="rs-a11y-listo">Listo</button></div>';

        var guia = document.createElement('div');
        guia.id = 'rs-a11y-guia';
        guia.hidden = true;
        guia.setAttribute('aria-hidden', 'true');

        document.body.appendChild(btn);
        document.body.appendChild(capa);
        document.body.appendChild(panel);
        document.body.appendChild(guia);

        // -------------------------------------------------------- eventos
        var ultimoFoco = null;

        function abrir() {
            ultimoFoco = document.activeElement;
            capa.hidden = false;
            panel.hidden = false;
            btn.setAttribute('aria-expanded', 'true');
            panel.querySelector('.rs-a11y-cerrar').focus();
        }

        function cerrar() {
            capa.hidden = true;
            panel.hidden = true;
            btn.setAttribute('aria-expanded', 'false');
            (ultimoFoco && ultimoFoco.focus ? ultimoFoco : btn).focus();
        }

        btn.addEventListener('click', function () { panel.hidden ? abrir() : cerrar(); });
        capa.addEventListener('click', cerrar);
        panel.querySelector('.rs-a11y-cerrar').addEventListener('click', cerrar);
        panel.querySelector('.rs-a11y-listo').addEventListener('click', cerrar);
        panel.querySelector('.rs-a11y-restablecer').addEventListener('click', function () {
            pref = Object.assign({}, BASE);
            guardar(pref); aplicar(); refrescar();
        });

        panel.addEventListener('click', function (e) {
            var b = e.target.closest('[data-op]');
            if (b) {
                var op = b.getAttribute('data-op');
                if (op.indexOf('color:') === 0) {
                    var c = op.slice(6);
                    pref.color = pref.color === c ? 'ninguno' : c;
                } else {
                    pref[op] = !pref[op];
                }
            }
            var t = e.target.closest('[data-tam]');
            if (t) pref.tam = Math.max(0, Math.min(TAM_MAX, pref.tam + parseInt(t.getAttribute('data-tam'), 10)));
            if (b || t) { guardar(pref); aplicar(); refrescar(); }
        });

        // Teclado: Escape cierra y el foco no sale del panel mientras está abierto.
        document.addEventListener('keydown', function (e) {
            if (panel.hidden) return;
            if (e.key === 'Escape') { e.preventDefault(); cerrar(); return; }
            if (e.key !== 'Tab') return;
            var f = panel.querySelectorAll('button:not([disabled])');
            var primero = f[0], ultimo = f[f.length - 1];
            if (e.shiftKey && document.activeElement === primero) { e.preventDefault(); ultimo.focus(); }
            else if (!e.shiftKey && document.activeElement === ultimo) { e.preventDefault(); primero.focus(); }
        });

        document.addEventListener('mousemove', function (e) {
            if (pref.guia) guia.style.top = (e.clientY - 22) + 'px';
        }, { passive: true });

        function refrescar() {
            panel.querySelectorAll('[data-op]').forEach(function (b) {
                var op = b.getAttribute('data-op');
                var activo = op.indexOf('color:') === 0 ? pref.color === op.slice(6) : !!pref[op];
                b.setAttribute('aria-pressed', activo ? 'true' : 'false');
            });
            var pct = 100 + pref.tam * 12.5;
            panel.querySelector('#rs-a11y-tam-valor').textContent = (pct % 1 ? pct.toFixed(1) : pct) + ' %';
            panel.querySelector('.rs-a11y-barra i').style.width = (pref.tam / TAM_MAX * 100) + '%';
            panel.querySelector('[data-tam="-1"]').disabled = pref.tam === 0;
            panel.querySelector('[data-tam="1"]').disabled = pref.tam === TAM_MAX;
        }

        refrescar();
        aplicar();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', construir);
    else construir();
})();