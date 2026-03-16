import esbuild from "esbuild";
import { sassPlugin } from "esbuild-sass-plugin";
import postcss from "postcss";
import postcssImport from "postcss-import";
import tailwindPostcss from "@tailwindcss/postcss";
import autoprefixer from "autoprefixer";
import fs from "fs-extra";
import path from "path";
import browserSync from "browser-sync";
const bs = browserSync.create();

async function copyDependencies() {
  const packageJson = await fs.readJson('./package.json');
  const dependencies = Object.keys(packageJson.dependencies || {});

  await Promise.all(
    dependencies.map(async (dependency) => {
      const dependencyPath = path.join('./node_modules', dependency);
      const distPath = path.join(dependencyPath, 'dist');
      const libPath = path.join('./wwwroot/assets/libs', dependency);

      if (await fs.pathExists(distPath)) {
        await fs.copy(distPath, libPath);
      } else {
        await fs.copy(dependencyPath, libPath);
      }
    })
  );
}

const ctx = esbuild.build({
  logLevel: "debug",
  metafile: true,
  entryPoints: [
    "wwwroot/assets/scss/style.scss"
  ],
  outfile: "wwwroot/assets/css/style.css",
  allowOverwrite: true,
  bundle: true,
  plugins: [
    sassPlugin({
      async transform(source) {
        // Ensure postcss is defined and use postcss-import + Tailwind's PostCSS adapter + autoprefixer
        const result = await postcss([postcssImport, tailwindPostcss, autoprefixer]).process(source, { from: undefined });
        return result.css;
      },
    }),
  ],
  loader: {
    ".png": "file",
    ".jpg": "file",
    ".jpeg": "file",
    ".svg": "file",
    ".gif": "file",
    ".woff": "file",
    ".ttf": "file",
    ".eot": "file",
    ".woff2": "file",
    ".html": "file",
  },
});

ctx.then(async () => {
  console.log("⚡ Styles & Scripts Compiled! ⚡ ");
  // To Libs Dependencies
  await copyDependencies().then(() => {
    console.log("⚡ libs Compiled! ⚡ ");
  });
}).catch((err) => {
  console.error(err);
  process.exit(1);
});
