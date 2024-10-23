let { XMLParser, XMLBuilder } = require("fast-xml-parser");
let fs = require('fs')
let path = require('path')
let _ = require('lodash');



function main() {
  let vanilla = parser.parse(fs.readFileSync(vanillaDefs)).contentpackage

  let all = {}
  collectItems(vanilla, all)
  collectStructures(vanilla, all)
  collectDecals(vanilla, all)
  collectLevelObjects(vanilla, all)
  collectParticles(vanilla, all)

  fs.writeFileSync('all.js', `var all = ${JSON.stringify(all, null, 2)}`)
}

function collectItems(vanilla, all) {
  let itemFiles = vanilla.Item.map(i => i['@file'])

  all.items = {}
  let allItems = {}


  // put all items in one big pile
  for (let itemFile of itemFiles) {
    let raw = parser.parse(fs.readFileSync(path.join(barotraumaFolder, itemFile)))
    let items = raw['Items'] || raw['items']

    if (items) {
      for (let categoryName in items) {
        if (categoryName.startsWith('@')) continue

        let category = items[categoryName]
        if (!Array.isArray(category)) category = [category]

        for (let item of category) {
          item.file = itemFile
          allItems[item["@identifier"]] = item
        }
      }
    }

    // just for ladder
    if (raw['Item']) {
      let item = raw['Item']
      item.file = itemFile
      allItems[item["@identifier"]] = item
    }
  }

  //console.log(allItems);

  //analyse
  for (let id in allItems) {
    item = allItems[id]
    let sprites
    try {
      if (item['@variantof']) {
        let prototype = allItems[item['@variantof']]
        sprites = prototype['Sprite'] || prototype['sprite']
        item.file = prototype.file
      } else {
        sprites = item['Sprite'] || item['sprite']
      }
    } catch (e) {
      console.log(item['@variantof'], e);
    }

    if (!Array.isArray(sprites)) sprites = [sprites]

    try {
      sprites = sprites.map(s => {
        let texture = s['@texture']
        if (!texture.startsWith('Content')) {
          texture = path.join(path.dirname(item.file), texture)
        }

        //texture = path.join(barotraumaFolder, texture)
        texture = path.relative('Content', texture)

        return {
          texture,
          sourcerect: s['@sourcerect'],
        }
      })
    } catch (e) {
      console.log('@variantof' in item, item.file, item["@identifier"]);
    }

    // stupid genetic materials don't have soucerects
    if (!sprites.every(s => s.sourcerect != null)) continue

    all.items[item["@identifier"]] = sprites
  }
}

function collectStructures(vanilla, all) {
  let structureFiles = vanilla.Structure.map(i => i['@file'])

  all.structures = {}

  for (let structuresFile of structureFiles) {
    let prefabs = parser.parse(fs.readFileSync(path.join(barotraumaFolder, structuresFile))).prefabs

    for (let category in prefabs) {
      if (category.startsWith('@')) continue
      category = prefabs[category]

      if (!Array.isArray(category)) category = [category]
      for (let structure of category) {

        // to avoid that pesky "a"
        if (typeof structure != 'object') continue

        let sprites = structure['sprite'] || structure['Sprite']

        if (!Array.isArray(sprites)) sprites = [sprites]

        try {
          sprites = sprites.map(s => {
            let texture = s['@texture']
            if (!texture.startsWith('Content')) {
              texture = path.join(path.dirname(structuresFile), texture)
            }

            //texture = path.join(barotraumaFolder, texture)
            texture = path.relative('Content', texture)

            return {
              texture,
              body: structure['@body'] == 'true',
              sourcerect: s['@sourcerect'],
            }
          })
        } catch (e) {
          console.log(structuresFile, category, structure);
        }

        all.structures[structure["@identifier"]] = sprites
      }
    }
  }

}

function collectDecals(vanilla, all) {
  let decalsFile = vanilla.Decals['@file']

  all.decals = {}

  let prefabs = parser.parse(fs.readFileSync(path.join(barotraumaFolder, decalsFile))).prefabs

  for (let key in prefabs) {
    let decal = prefabs[key]
    if (Array.isArray(decal)) decal = decal[0]

    let sprites = key == 'grime' ? decal : decal['sprite']
    if (!Array.isArray(sprites)) sprites = [sprites]

    try {
      sprites = sprites.map(s => {
        let texture = s['@texture']
        if (!texture.startsWith('Content')) {
          texture = path.join(path.dirname(decalsFile), texture)
        }

        //texture = path.join(barotraumaFolder, texture)
        texture = path.relative('Content', texture)

        return {
          texture,
          sourcerect: s['@sourcerect'],
        }
      })
    } catch (e) {
      console.log(decal);
    }

    all.decals[key] = sprites
  }
}

function collectParticles(vanilla, all) {
  let particlesFile = vanilla.Particles['@file']

  all.particles = {}

  let prefabs = parser.parse(fs.readFileSync(path.join(barotraumaFolder, particlesFile))).prefabs

  for (let key in prefabs) {
    let sprites = prefabs[key]['sprite'] || prefabs[key]['Sprite'] || prefabs[key]['animatedsprite']
    if (!Array.isArray(sprites)) sprites = [sprites]

    try {
      sprites = sprites.map(s => {
        let texture = s['@texture']
        if (!texture.startsWith('Content')) {
          texture = path.join(path.dirname(particlesFile), texture)
        }

        //texture = path.join(barotraumaFolder, texture)
        texture = path.relative('Content', texture)

        return {
          texture,
          sourcerect: s['@sourcerect'],
        }
      })
    } catch (e) {
      console.log(key);
    }

    all.particles[key] = sprites
  }
}

function collectLevelObjects(vanilla, all) {
  let objectFiles = vanilla.LevelObjectPrefabs.map(i => i['@file'])

  all.levelObjects = {}

  for (let objectFile of objectFiles) {
    let categories = parser.parse(fs.readFileSync(path.join(barotraumaFolder, objectFile))).levelobjects

    for (let category in categories) {
      category = categories[category]
      if (!Array.isArray(category)) category = [category]

      for (let o of category) {
        let sprites = o['Sprite'] || o['sprite'] || o['DeformableSprite']
        if (sprites) {

          if (!Array.isArray(sprites)) sprites = [sprites]

          sprites = sprites.map(s => {
            let texture = s['@texture']
            if (!texture.startsWith('Content')) {
              texture = path.join(path.dirname(objectFile), texture)
            }

            //texture = path.join(barotraumaFolder, texture)
            texture = path.relative('Content', texture)

            return {
              texture,
              sourcerect: s['@sourcerect'],
            }
          })

          all.levelObjects[o["@identifier"]] = sprites
        }
      }
    }
  }
}


let barotraumaFolder = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma"
let contentFolder = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma/Content"
let vanillaDefs = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma/Content/ContentPackages/Vanilla.xml"

let parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: "@",
  //commentPropName: '#',
});


main()

