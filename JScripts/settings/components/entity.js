window.component.Entity = function (key, data) {
  let entity = ce({ className: 'entity' })

  entity.append(ce({ innerHTML: key }))

  if (data[0].body) {

    entity.classList.add('hasBody')
    entity.append(ce({ innerHTML: `has physical body!`, className: `bodyWarning` }))
  }


  let imgs = ce({ className: 'images' })



  for (let sprite of data) {
    let img = ce({ name: 'img' })
    img.src = contentFolder + sprite.texture
    img.title = key

    try {
      if (sprite.sourcerect) {
        let rect = sprite.sourcerect.split(',')
        img.style = `width:${rect[2]}px; height:${rect[3]}px; object-position: ${-rect[0]}px ${-rect[1]}px;`
      }
    } catch (e) {
      console.log(key, data);
    }

    imgs.append(img)
  }

  entity.append(imgs)




  Object.defineProperty(entity, "Enabled", {
    get() {
      return entity.enabled;
    },
    set(newValue) {
      entity.enabled = newValue;
      if (entity.enabled) entity.classList.add('enabled')
      if (!entity.enabled) entity.classList.remove('enabled')
    },
  });

  entity.addEventListener('mousedown', (e) => {
    entity.Enabled = !entity.Enabled
    mouse.pressed = true
    mouse.enabling = entity.Enabled
  })

  entity.addEventListener('mouseenter', (e) => {
    if (mouse.pressed) {
      entity.Enabled = mouse.enabling
    }
  })

  entity.Enabled = true

  return entity
}