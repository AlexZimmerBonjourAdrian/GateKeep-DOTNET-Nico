if(!self.define){let e,s={};const a=(a,i)=>(a=new URL(a+".js",i).href,s[a]||new Promise(s=>{if("document"in self){const e=document.createElement("script");e.src=a,e.onload=s,document.head.appendChild(e)}else e=a,importScripts(a),s()}).then(()=>{let e=s[a];if(!e)throw new Error(`Module ${a} didn’t register its module`);return e}));self.define=(i,c)=>{const n=e||("document"in self?document.currentScript.src:"")||location.href;if(s[n])return;let t={};const f=e=>a(e,n),r={module:{uri:n},exports:t,require:f};s[n]=Promise.all(i.map(e=>r[e]||f(e))).then(e=>(c(...e),t))}}define(["./workbox-4754cb34"],function(e){"use strict";importScripts(),self.skipWaiting(),e.clientsClaim(),e.precacheAndRoute([{url:"/_next/app-build-manifest.json",revision:"b6ba60be61cf976ac438d80472d22440"},{url:"/_next/static/chunks/125-87d232e2c7a61fe6.js",revision:"87d232e2c7a61fe6"},{url:"/_next/static/chunks/139.7a5a8e93a21948c1.js",revision:"7a5a8e93a21948c1"},{url:"/_next/static/chunks/255-6080d22baa93e028.js",revision:"6080d22baa93e028"},{url:"/_next/static/chunks/29-2f4699366392b422.js",revision:"2f4699366392b422"},{url:"/_next/static/chunks/4bd1b696-21f374d1156f834a.js",revision:"21f374d1156f834a"},{url:"/_next/static/chunks/509-3ebaf7d526336866.js",revision:"3ebaf7d526336866"},{url:"/_next/static/chunks/514.606a05edffbb39e1.js",revision:"606a05edffbb39e1"},{url:"/_next/static/chunks/646.f342b7cffc01feb0.js",revision:"f342b7cffc01feb0"},{url:"/_next/static/chunks/729-e6af1306e35e73cc.js",revision:"e6af1306e35e73cc"},{url:"/_next/static/chunks/796-f31358836a74a2bf.js",revision:"f31358836a74a2bf"},{url:"/_next/static/chunks/7cb1fa1f.32fc9056ac653916.js",revision:"32fc9056ac653916"},{url:"/_next/static/chunks/aaea2bcf-007ddbe6e1307a81.js",revision:"007ddbe6e1307a81"},{url:"/_next/static/chunks/app/_not-found/page-f6aa4c8accaa4667.js",revision:"f6aa4c8accaa4667"},{url:"/_next/static/chunks/app/anuncio/%5Bid%5D/page-fbc90f106f3f83dd.js",revision:"fbc90f106f3f83dd"},{url:"/_next/static/chunks/app/anuncio/crearAnuncio/page-1b26315b3d5e146d.js",revision:"1b26315b3d5e146d"},{url:"/_next/static/chunks/app/anuncio/listadoAnuncios/page-ed56560ba76b8e45.js",revision:"ed56560ba76b8e45"},{url:"/_next/static/chunks/app/beneficio/%5Bid%5D/page-41f1f81e2399047c.js",revision:"41f1f81e2399047c"},{url:"/_next/static/chunks/app/beneficio/crearBeneficio/page-c18a3c097384fe2e.js",revision:"c18a3c097384fe2e"},{url:"/_next/static/chunks/app/beneficio/listadoBeneficios/page-ad137c42b3bb68e8.js",revision:"ad137c42b3bb68e8"},{url:"/_next/static/chunks/app/edificios/%5Bid%5D/page-5fa3d69895e3675c.js",revision:"5fa3d69895e3675c"},{url:"/_next/static/chunks/app/edificios/crearEdificio/page-a2474d7651f183cc.js",revision:"a2474d7651f183cc"},{url:"/_next/static/chunks/app/edificios/listadoEdificios/page-773a2967bf845065.js",revision:"773a2967bf845065"},{url:"/_next/static/chunks/app/evento/%5Bid%5D/page-e925c3fca43169ee.js",revision:"e925c3fca43169ee"},{url:"/_next/static/chunks/app/evento/crearEvento/page-71f9f9005324bfaf.js",revision:"71f9f9005324bfaf"},{url:"/_next/static/chunks/app/evento/listadoEventos/page-b9380327f90eabf0.js",revision:"b9380327f90eabf0"},{url:"/_next/static/chunks/app/layout-62fb0a1df9bd42de.js",revision:"62fb0a1df9bd42de"},{url:"/_next/static/chunks/app/login/page-c66fbd591db20d91.js",revision:"c66fbd591db20d91"},{url:"/_next/static/chunks/app/notificaciones/page-7a38339df8615d19.js",revision:"7a38339df8615d19"},{url:"/_next/static/chunks/app/page-34b6fb4fac3e0dd5.js",revision:"34b6fb4fac3e0dd5"},{url:"/_next/static/chunks/app/perfil/escaner/page-e10cfb184ca859d4.js",revision:"e10cfb184ca859d4"},{url:"/_next/static/chunks/app/perfil/page-6789496e5c02fd73.js",revision:"6789496e5c02fd73"},{url:"/_next/static/chunks/app/register/page-d0968c172bd5d8ea.js",revision:"d0968c172bd5d8ea"},{url:"/_next/static/chunks/app/reglas-acceso/%5Bid%5D/page-0b84509bb2834b7a.js",revision:"0b84509bb2834b7a"},{url:"/_next/static/chunks/app/reglas-acceso/crearReglaAcceso/page-bdfb9fe529c52697.js",revision:"bdfb9fe529c52697"},{url:"/_next/static/chunks/app/reglas-acceso/editarReglaAcceso/%5Bid%5D/page-05126cb3d452bb41.js",revision:"05126cb3d452bb41"},{url:"/_next/static/chunks/app/reglas-acceso/listadoReglasAcceso/page-72185a5f096ac140.js",revision:"72185a5f096ac140"},{url:"/_next/static/chunks/app/salones/crearSalon/page-931224b091f328ac.js",revision:"931224b091f328ac"},{url:"/_next/static/chunks/app/salones/listadoSalones/page-209ba8090be9a855.js",revision:"209ba8090be9a855"},{url:"/_next/static/chunks/framework-a6e0b7e30f98059a.js",revision:"a6e0b7e30f98059a"},{url:"/_next/static/chunks/main-2ebb06bfa1cff436.js",revision:"2ebb06bfa1cff436"},{url:"/_next/static/chunks/main-app-c37052e532f8e346.js",revision:"c37052e532f8e346"},{url:"/_next/static/chunks/pages/_app-82835f42865034fa.js",revision:"82835f42865034fa"},{url:"/_next/static/chunks/pages/_error-013f4188946cdd04.js",revision:"013f4188946cdd04"},{url:"/_next/static/chunks/polyfills-42372ed130431b0a.js",revision:"846118c33b2c0e922d7b3a7676f81f6f"},{url:"/_next/static/chunks/webpack-a3d45ae443cbfed6.js",revision:"a3d45ae443cbfed6"},{url:"/_next/static/css/24fc923e4b19c2ff.css",revision:"24fc923e4b19c2ff"},{url:"/_next/static/css/2dcff1f885037de5.css",revision:"2dcff1f885037de5"},{url:"/_next/static/css/5f049d1f8dea2762.css",revision:"5f049d1f8dea2762"},{url:"/_next/static/css/ba5c10f1bf62ba08.css",revision:"ba5c10f1bf62ba08"},{url:"/_next/static/media/InterVariable-Italic.ef0ecaff.woff2",revision:"ef0ecaff"},{url:"/_next/static/media/InterVariable.ff710c09.woff2",revision:"ff710c09"},{url:"/_next/static/media/LogoGateKeep.af78ba5a.webp",revision:"52f14afaabe73b1eb1d4d02f5e4f7bb8"},{url:"/_next/static/media/primeicons.310a7310.ttf",revision:"310a7310"},{url:"/_next/static/media/primeicons.7f772274.woff",revision:"7f772274"},{url:"/_next/static/media/primeicons.8ca441e1.eot",revision:"8ca441e1"},{url:"/_next/static/media/primeicons.e1a53edb.woff2",revision:"e1a53edb"},{url:"/_next/static/media/primeicons.ff09de3f.svg",revision:"ff09de3f"},{url:"/_next/static/nnvLDsYwRzufIbseU9wsw/_buildManifest.js",revision:"50f40ef64f39acb46397a35f8fbd77d6"},{url:"/_next/static/nnvLDsYwRzufIbseU9wsw/_ssgManifest.js",revision:"b6652df95db52feb4daf4eca35380933"},{url:"/_redirects",revision:"6a02faf7ea2a9584134ffe15779a0e44"},{url:"/assets/Harvard.webp",revision:"6715931c9292ec55956a52fdcb94db77"},{url:"/assets/LogoGateKeep.webp",revision:"52f14afaabe73b1eb1d4d02f5e4f7bb8"},{url:"/assets/basketball-icon.svg",revision:"a32125c80f7d16f938354475022bf5b3"},{url:"/assets/react.svg",revision:"d41d8cd98f00b204e9800998ecf8427e"},{url:"/browserconfig.xml",revision:"1d5264c55cf373f90638a2d74e9c6e93"},{url:"/logo.svg",revision:"87ffdcad14a2274acb893535f16e6957"},{url:"/manifest.json",revision:"1cbf8e1d6c3cfa5d62adda756c5732e7"},{url:"/offline.html",revision:"a565eaeda752748af3c55be90c72cec0"},{url:"/sql-wasm.wasm",revision:"adf3b772bd5b646d050ce90d58b3db23"}],{ignoreURLParametersMatching:[]}),e.cleanupOutdatedCaches(),e.registerRoute("/",new e.NetworkFirst({cacheName:"start-url",plugins:[{cacheWillUpdate:async({request:e,response:s,event:a,state:i})=>s&&"opaqueredirect"===s.type?new Response(s.body,{status:200,statusText:"OK",headers:s.headers}):s}]}),"GET"),e.registerRoute(/^https:\/\/fonts\.(?:gstatic)\.com\/.*/i,new e.CacheFirst({cacheName:"google-fonts-webfonts",plugins:[new e.ExpirationPlugin({maxEntries:4,maxAgeSeconds:31536e3})]}),"GET"),e.registerRoute(/^https:\/\/fonts\.(?:googleapis)\.com\/.*/i,new e.StaleWhileRevalidate({cacheName:"google-fonts-stylesheets",plugins:[new e.ExpirationPlugin({maxEntries:4,maxAgeSeconds:604800})]}),"GET"),e.registerRoute(/\.(?:eot|otf|ttc|ttf|woff|woff2|font.css)$/i,new e.StaleWhileRevalidate({cacheName:"static-font-assets",plugins:[new e.ExpirationPlugin({maxEntries:4,maxAgeSeconds:604800})]}),"GET"),e.registerRoute(/\.(?:jpg|jpeg|gif|png|svg|ico|webp)$/i,new e.StaleWhileRevalidate({cacheName:"static-image-assets",plugins:[new e.ExpirationPlugin({maxEntries:64,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\/_next\/image\?url=.+$/i,new e.StaleWhileRevalidate({cacheName:"next-image",plugins:[new e.ExpirationPlugin({maxEntries:64,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\.(?:mp3|wav|ogg)$/i,new e.CacheFirst({cacheName:"static-audio-assets",plugins:[new e.RangeRequestsPlugin,new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\.(?:mp4)$/i,new e.CacheFirst({cacheName:"static-video-assets",plugins:[new e.RangeRequestsPlugin,new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\.(?:js)$/i,new e.StaleWhileRevalidate({cacheName:"static-js-assets",plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\.(?:css|less)$/i,new e.StaleWhileRevalidate({cacheName:"static-style-assets",plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\/_next\/data\/.+\/.+\.json$/i,new e.StaleWhileRevalidate({cacheName:"next-data",plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(/\.(?:json|xml|csv)$/i,new e.NetworkFirst({cacheName:"static-data-assets",plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(({url:e})=>{if(!(self.origin===e.origin))return!1;const s=e.pathname;return!s.startsWith("/api/auth/")&&!!s.startsWith("/api/")},new e.NetworkFirst({cacheName:"apis",networkTimeoutSeconds:10,plugins:[new e.ExpirationPlugin({maxEntries:16,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(({url:e})=>{if(!(self.origin===e.origin))return!1;return!e.pathname.startsWith("/api/")},new e.NetworkFirst({cacheName:"others",networkTimeoutSeconds:10,plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:86400})]}),"GET"),e.registerRoute(({url:e})=>!(self.origin===e.origin),new e.NetworkFirst({cacheName:"cross-origin",networkTimeoutSeconds:10,plugins:[new e.ExpirationPlugin({maxEntries:32,maxAgeSeconds:3600})]}),"GET")});

// If the loader is already loaded, just stop.
if (!self.define) {
  let registry = {};

  // Used for `eval` and `importScripts` where we can't get script URL by other means.
  // In both cases, it's safe to use a global var because those functions are synchronous.
  let nextDefineUri;

  const singleRequire = (uri, parentUri) => {
    uri = new URL(uri + ".js", parentUri).href;
    return registry[uri] || (
      
        new Promise(resolve => {
          if ("document" in self) {
            const script = document.createElement("script");
            script.src = uri;
            script.onload = resolve;
            document.head.appendChild(script);
          } else {
            nextDefineUri = uri;
            importScripts(uri);
            resolve();
          }
        })
      
      .then(() => {
        let promise = registry[uri];
        if (!promise) {
          throw new Error(`Module ${uri} didn’t register its module`);
        }
        return promise;
      })
    );
  };

  self.define = (depsNames, factory) => {
    const uri = nextDefineUri || ("document" in self ? document.currentScript.src : "") || location.href;
    if (registry[uri]) {
      // Module is already loading or loaded.
      return;
    }
    let exports = {};
    const require = depUri => singleRequire(depUri, uri);
    const specialDeps = {
      module: { uri },
      exports,
      require
    };
    registry[uri] = Promise.all(depsNames.map(
      depName => specialDeps[depName] || require(depName)
    )).then(deps => {
      factory(...deps);
      return exports;
    });
  };
}
define(['./workbox-e43f5367'], (function (workbox) { 'use strict';

  importScripts();
  self.skipWaiting();
  workbox.clientsClaim();
  workbox.registerRoute("/", new workbox.NetworkFirst({
    "cacheName": "start-url",
    plugins: [{
      cacheWillUpdate: async ({
        request,
        response,
        event,
        state
      }) => {
        if (response && response.type === 'opaqueredirect') {
          return new Response(response.body, {
            status: 200,
            statusText: 'OK',
            headers: response.headers
          });
        }
        return response;
      }
    }]
  }), 'GET');
  workbox.registerRoute(/.*/i, new workbox.NetworkOnly({
    "cacheName": "dev",
    plugins: []
  }), 'GET');

}));
