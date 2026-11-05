"""Original project geometry. Run in Blender's scripting workspace or through Blender MCP.
Reference dimensions and deviations are recorded in Documentation/Validation/WallAlarm.md.
Does not remove the user's existing scenes. Exports selected equipment only, in metres.
"""
import bpy, math, os, json
from mathutils import Vector

ROOT = 'E:/projects/T3_LWS'
OUT = ROOT + '/Assets/_02LWS/Art/Models/FireAlarm'
os.makedirs(OUT, exist_ok=True)
os.makedirs(OUT+'/Source~', exist_ok=True)
scene = bpy.data.scenes.get('LWS_WallAlarm')
if scene and len(scene.objects):
    raise RuntimeError('LWS_WallAlarm already has objects; inspect before rebuilding')
scene = scene or bpy.data.scenes.new('LWS_WallAlarm')
bpy.context.window.scene = scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
collection = bpy.data.collections.new('LWS_Equipment')
scene.collection.children.link(collection)
stage = bpy.data.collections.new('LWS_PreviewOnly')
scene.collection.children.link(stage)
parts = []

def activate(o):
    bpy.ops.object.select_all(action='DESELECT')
    o.select_set(True)
    bpy.context.view_layer.objects.active = o

def move_to(o, target):
    for c in list(o.users_collection): c.objects.unlink(o)
    target.objects.link(o)

def material(name, color, metallic=0, rough=.4, emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1)
    m.use_nodes=True; p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Metallic'].default_value=metallic
    p.inputs['Roughness'].default_value=rough
    if emission:
        p.inputs['Emission Color'].default_value=(*color,1)
        p.inputs['Emission Strength'].default_value=emission
    return m

red=material('Alarm_EnamelRed',(.35,.004,.009),.15,.3)
dark=material('Alarm_DarkTrim',(.018,.022,.025),.35,.36)
steel=material('Alarm_BrushedMetal',(.38,.41,.43),.72,.38)
white=material('Alarm_Lettering',(.94,.93,.85),0,.6)
lens=material('Alarm_RedLens',(.24,.001,.003),.1,.22,.3)
capmat=material('Alarm_PressPlate',(.72,.65,.48),.05,.34)

def finish(o,name,mat,bevel=0,smooth=False):
    o.name=name; move_to(o,collection); o.data.materials.append(mat)
    activate(o); bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Manufactured edge','BEVEL');mod.width=bevel;mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    if smooth:
        for poly in o.data.polygons: poly.use_smooth=True
    parts.append(o);return o

def box(name,pos,size,mat,bevel=.001):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
    o=bpy.context.object;o.dimensions=size
    return finish(o,name,mat,bevel)

def disc(name,pos,radius,depth,mat,vertices=64):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,
        location=pos,rotation=(math.pi/2,0,0))
    return finish(bpy.context.object,name,mat,.0007,True)

def lathe(name,z,profile,mat,segments=64):
    verts=[];faces=[]
    for r,y in profile:
        verts.extend([(r*math.cos(t*2*math.pi/segments),y,z+r*math.sin(t*2*math.pi/segments)) for t in range(segments)])
    for k in range(len(profile)-1):
        for i in range(segments):
            a=k*segments+i;b=k*segments+(i+1)%segments
            faces.append((a,b,b+segments,a+segments))
    faces += [tuple(reversed(range(segments))),tuple((len(profile)-1)*segments+i for i in range(segments))]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);collection.objects.link(o);o.data.materials.append(mat)
    import bmesh
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    for p in mesh.polygons:p.use_smooth=True
    parts.append(o);return o

font=bpy.data.fonts.load(ROOT+'/Assets/_02LWS/UI/Fonts/NotoSansCJKkr-Regular.otf')
def text(name,body,pos,size,mat=white):
    curve=bpy.data.curves.new(name,'FONT');curve.body=body;curve.font=font
    curve.size=size;curve.align_x='CENTER';curve.align_y='CENTER';curve.resolution_u=1
    o=bpy.data.objects.new(name,curve);collection.objects.link(o)
    o.location=pos;o.rotation_euler=(math.pi/2,0,0);curve.materials.append(mat)
    bpy.context.view_layer.update()
    if o.dimensions.y>0: o.scale*=size/o.dimensions.y
    activate(o);bpy.ops.object.convert(target='MESH');o=bpy.context.object
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    parts.append(o);return o

# A wall-backed equipment plate: alarm above, steady location lamp, call point below.
box('Steel backplate',(0,-.009,.13),(.20,.018,.49),steel,.004)
box('Red faceplate',(0,-.020,.13),(.19,.006,.48),red,.003)
for x in [-.083,.083]:
    for z in [-.094,.354]:
        disc('Mount screw',(x,-.026,z),.0032,.002,steel,16)
        box('Screw slot',(x,-.0273,z),(.0038,.0005,.0007),dark,.0001)

# 144 mm round manual call point, central moving press plate, trim and phone cover.
disc('CallPoint rim',(0,-.035,0),.072,.025,dark)
disc('CallPoint housing',(0,-.040,0),.070,.029,red)
disc('Press socket',(0,-.056,0),.030,.007,dark)
disc('Press bezel',(0,-.060,0),.0275,.004,steel)
cap=disc('MovingCap',(0,-.064,0),.025,.006,capmat)
text('CallPoint title','발 신 기',(0,-.0555,.047),.010)
text('CallPoint instruction','화재시 강하게 누르시오',(0,-.0555,-.038),.0055)
text('CallPoint English','FIRE ALARM',(0,-.0555,-.053),.0048)
disc('Telephone cover',(-.045,-.056,-.005),.006,.002,dark,24)
text('Telephone label','전화',(-.045,-.057,-.016),.0045)
disc('Response LED',(.045,-.056,.003),.003,.002,lens,20)
for x in [-.055,.055]:
    disc('CallPoint screw',(x,-.056,.029),.0024,.0015,steel,16)

# Ribbed red position lamp. No flashing/strobe behavior is invented.
disc('Lamp base',(0,-.031,.153),.0435,.019,dark)
lathe('Red location lens',.153,[(.039,-.036),(.039,-.044),(.034,-.055),(.027,-.061),(.009,-.064)],lens)
text('Lamp caption','위치 표시등',(0,-.024,.098),.0055)

# 120 mm spun-metal bell: a stepped rim and convex shell rather than a cube.
disc('Bell motor base',(0,-.033,.287),.039,.022,dark)
lathe('Bell gong',.287,[(.057,-.031),(.060,-.034),(.060,-.039),(.057,-.050),(.049,-.064),(.033,-.075),(.008,-.080)],red)
disc('Gong center washer',(0,-.081,.287),.0055,.0015,steel,24)
disc('Gong center screw',(0,-.083,.287),.0025,.002,dark,16)
text('Bell caption','경 종',(0,-.024,.211),.007)

# Join static pieces to one object; keep the button independent for Unity animation.
static=[o for o in parts if o!=cap]
bpy.ops.object.select_all(action='DESELECT')
for o in static:o.select_set(True)
bpy.context.view_layer.objects.active=static[0];bpy.ops.object.join()
housing=bpy.context.object;housing.name='Housing'
scene.cursor.location=(0,0,0);activate(housing);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
root=bpy.data.objects.new('WallAlarmModel',None);collection.objects.link(root)
housing.parent=root;cap.parent=root

# Preview-only wall, camera and soft lighting are never exported to FBX.
wallmat=material('Preview_Wall',(.56,.57,.56),0,.85)
bpy.ops.mesh.primitive_cube_add(size=1,location=(0,.045,.14))
wall=bpy.context.object;wall.name='PreviewWall';wall.dimensions=(.8,.09,.9)
move_to(wall,stage);wall.data.materials.append(wallmat)
def aim(o,point):o.rotation_euler=(Vector(point)-o.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('PreviewCamera');cam=bpy.data.objects.new('PreviewCamera',camdata);stage.objects.link(cam)
cam.location=(.45,-.85,.39);aim(cam,(0,-.02,.13));camdata.type='ORTHO';camdata.ortho_scale=.65;scene.camera=cam
for name,loc,power,size in [('Key',(-.5,-.6,.8),18,.6),('Fill',(.6,-.3,.2),5,.5)]:
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size
    o=bpy.data.objects.new(name,d);stage.objects.link(o);o.location=loc;aim(o,(0,0,.12))
scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=900;scene.render.resolution_y=1100
scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('LWS_PreviewWorld');scene.world.use_nodes=True
bg=next(n for n in scene.world.node_tree.nodes if n.type=='BACKGROUND')
bg.inputs[0].default_value=(.25,.25,.25,1)
bg.inputs[1].default_value=.4
scene.view_settings.view_transform='AgX'
scene.render.image_settings.file_format='PNG'
bpy.ops.object.select_all(action='DESELECT')
for o in [root,housing,cap]:o.select_set(True)
bpy.context.view_layer.objects.active=root
bpy.ops.export_scene.fbx(filepath=OUT+'/WallAlarm.fbx',use_selection=True,
    object_types={'EMPTY','MESH'},global_scale=1,apply_unit_scale=True,
    axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False)
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=OUT+'/Source~/WallAlarm.blend')
scene.render.filepath=ROOT+'/.utmp/alarm-reference/model-oblique.png'
bpy.ops.render.render(write_still=True)
cam.location=(0,-1,.13);aim(cam,(0,0,.13));scene.render.filepath=ROOT+'/.utmp/alarm-reference/model-front.png'
bpy.ops.render.render(write_still=True)
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_distance=.8
            area.spaces.active.region_3d.view_location=(0,0,.13)
mesh_stats={o.name:{'vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'dimensions':list(o.dimensions)} for o in [housing,cap]}
with open(OUT+'/ModelInfo.json','w',encoding='utf8') as f:json.dump({'blender':bpy.app.version_string,'metres':True,'objects':mesh_stats,'references':'See Documentation/Validation/WallAlarm.md','original_scene_preserved':'Scene'},f,indent=2)
result={'scene':scene.name,'output':OUT,'meshes':mesh_stats}
