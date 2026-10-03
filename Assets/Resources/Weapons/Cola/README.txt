Place the exported transparent PNG files in this folder with these exact names:
- dik.png      (club idle)
- kalkik.png   (club attack frame 1)
- inik.png     (club attack frame 2)
- elde.png     (soda mode idle)
- ates.png     (soda fire)
- yerde.png    (world pickup)

The runtime loads them as Texture2D resources and creates sprites, so the PNGs do not need manual Sprite import settings. Keep their alpha transparency. The project scripts expect these files under Assets/Resources/Weapons/Cola/.
