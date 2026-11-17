"""Original school architecture. Run in the connected Blender; only owned collection is rebuilt."""
import bpy, math, os, json
from mathutils import Vector
ROOT='E:/projects/T3_LWS/Assets/_02LWS/Art/Models/SchoolArchitecture'
COLLECTION='LWS_SchoolArchitecture'

def material(name,color,metal=0,rough=.5):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color=(*color,1);m.use_nodes=True
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough
    return m

def link_owned(obj):
    for c in list(obj.users_collection):c.objects.unlink(obj)
    collection.objects.link(obj)

def group(name):
    o=bpy.data.objects.new(name,None);collection.objects.link(o);return o

def box(name,dim,loc,mat,parent,bevel=.002):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object;o.name=name;link_owned(o);o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mat);o.parent=parent
    if bevel>0:
        mod=o.modifiers.new('Small manufactured edge','BEVEL');mod.width=bevel;mod.segments=2
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
        mod=o.modifiers.new('Face normals','WEIGHTED_NORMAL');mod.keep_sharp=True
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def tube(name,a,b,r,mat,parent):
    a=Vector(a);b=Vector(b);v=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=v.length,location=(a+b)/2)
    o=bpy.context.object;o.name=name;link_owned(o);o.rotation_euler=v.to_track_quat('Z','Y').to_euler();o.parent=parent;o.data.materials.append(mat)
    for p in o.data.polygons:p.use_smooth=True
    return o

def frame(name,w,h,depth,parent,front=-.035,bar=.06):
    box(name+'Left',(bar,depth,h+bar),(-w/2-bar/2,front,h/2),metal,parent)
    box(name+'Right',(bar,depth,h+bar),(w/2+bar/2,front,h/2),metal,parent)
    box(name+'Top',(w+2*bar,depth,bar),(0,front,h+bar/2),metal,parent)
    box(name+'StopLeft',(.014,.012,h),(-w/2+.005,front-depth/2-.008,h/2),seal,parent,.001)
    box(name+'StopRight',(.014,.012,h),(w/2-.005,front-depth/2-.008,h/2),seal,parent,.001)

def lever(w,h,parent):
    x=w/2-.12;z=1.0
    tube('Handle spindle',(x,-.02,z),(x,-.105,z),.018,metal,parent)
    tube('Rounded lever',(x,-.105,z),(x-.135,-.105,z),.012,metal,parent)
    tube('Lock escutcheon',(x,-.042,z-.1),(x,-.055,z-.1),.022,metal,parent)
    for z0 in (.32,h-.32):tube('Hinge',(-w/2-.018,0,z0-.045),(-w/2-.018,0,z0+.045),.012,metal,parent)

def door(name,w,h,thick,glazed=False,with_frame=True):
    g=group(name)
    if glazed:
        gw=.54;gh=.62;gc=1.55;bottom=gc-gh/2;top=gc+gh/2
        box('Door lower panel',(w,thick,bottom),(0,0,bottom/2),paint,g)
        box('Door upper panel',(w,thick,h-top),(0,0,(h+top)/2),paint,g)
        for s in (-1,1):box('Door side stile',((w-gw)/2,thick,gh),(s*(w+gw)/4,0,gc),paint,g)
        box('Observation glass',(gw-.022,.012,gh-.022),(0,-.004,gc),glass,g,.001)
        for s in (-1,1):
            box('Observation bead vertical',(.016,.014,gh),(s*(gw/2-.008),-.048,gc),metal,g,.001)
            box('Observation bead horizontal',(gw,.014,.016),(0,-.048,gc+s*(gh/2-.008)),metal,g,.001)
    else:
        box('Door leaf',(w,thick,h),(0,0,h/2),paint,g)
        for z in (.22,h-.22):box('Recessed panel line',(w-.16,.003,.008),(0,-thick/2-.001,z),seal,g,0)
    lever(w,h,g)
    if with_frame:frame('Frame',w+.02,h+.025,.23 if glazed else .12,g,-.10 if glazed else -.015)
    return g

def window():
    g=group('SchoolWindowSliding');w=3.25;h=1.72;bar=.055
    frame('Outer frame',w,h,.13,g,0,bar)
    box('Bottom sill',(w+.16,.20,.035),(0,-.025,-.025),metal,g)
    for y in (-.047,0,.047):box('Sliding track',(w,.009,.012),(0,y,.012),metal,g,.001)
    pw=w/4
    for i in range(4):
        x=-w/2+pw*(i+.5);y=-.025 if i%2 else .025
        box('Frosted glazing '+str(i),(pw-.035,.009,h-.055),(x,y,h/2),glass,g,.001)
        for s in (-1,1):
            box('Sash upright',(.022,.035,h-.012),(x+s*(pw/2-.011),y-.009,h/2),metal,g,.001)
            box('Sash horizontal',(pw,.035,.022),(x,y-.009,.012+(h-.024)*(s+1)/2),metal,g,.001)
        if i in (1,2):box('Window grip',(.018,.026,.18),(x+pw/2-.048,y-.037,.85),seal,g,.003)
    return g

def elevator():
    g=group('SchoolElevatorPortal')
    for s in (-1,1):
        box('Brushed metal door',(.67,.045,2.28),(s*.34,0,1.14),metal,g,.003)
        box('Center door seal',(.005,.009,2.25),(s*.004,-.028,1.14),seal,g,.0008)
        box('Portal jamb',(.12,.22,2.4),(s*.78,-.025,1.2),metal,g,.004)
        box('Portal inner reveal',(.018,.18,2.3),(s*.693,0,1.15),seal,g,.001)
    box('Portal header',(1.68,.22,.12),(0,-.025,2.34),metal,g,.004)
    box('Landing sill',(1.5,.25,.025),(0,-.025,.0125),metal,g,.001)
    for y in (-.07,-.02):box('Sill groove',(1.39,.006,.005),(0,y,.026),seal,g,0)
    return g

def stairwell():
    # Geometry in Blender X/Y ground, Z up. The longer 4.8m axis holds both landings.
    g=group('SchoolStairwellVisual')
    for i in range(10):
        top=(i+1)*.16;y=-1.15+i*.27
        box('Lower flight step',(1.25,.27,top),(1.4,y,top/2),stone,g,.001)
        box('Lower nosing',(1.25,.023,.012),(1.4,y-.127,top+.002),metal,g,.001)
        top=1.6+(i+1)*.16;y=1.28-i*.27
        box('Upper flight step',(1.25,.27,.16),(2.8,y,top-.08),stone,g,.001)
        box('Upper nosing',(1.25,.023,.012),(2.8,y+.127,top+.002),metal,g,.001)
    # Continuous concrete waist transfers the upper run into both landings.
    verts=[(2.175,1.415,1.42),(3.425,1.415,1.42),(3.425,-1.285,3.02),(2.175,-1.285,3.02),
           (2.175,1.415,1.6),(3.425,1.415,1.6),(3.425,-1.285,3.2),(2.175,-1.285,3.2)]
    faces=[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7),(0,3,7,4),(1,5,6,2)]
    mesh=bpy.data.meshes.new('Upper stair waist mesh');mesh.from_pydata(verts,[],faces);mesh.update()
    slab=bpy.data.objects.new('Continuous upper stair waist',mesh);collection.objects.link(slab);slab.parent=g;slab.data.materials.append(stone)
    box('Intermediate landing bearing beam',(2.65,.30,.26),(2.1,1.45,1.32),stone,g,.002)
    box('Upper landing bearing beam',(2.65,.30,.22),(2.1,-1.37,2.94),stone,g,.002)
    for x in (.90,3.30):box('Intermediate landing concrete pier',(.22,.30,1.44),(x,2.05,.72),stone,g,.002)
    box('Intermediate landing',(2.65,.99,.16),(2.1,1.905,1.52),stone,g,.001)
    box('Upper landing',(2.65,1.12,.16),(2.1,-1.84,3.12),stone,g,.001)
    box('Rear wall',(.16,4.8,6.4),(3.92,0,3.2),wall,g,0)
    for s in (-1,1):box('Side wall',(4,.16,6.4),(2,s*2.32,3.2),wall,g,0)
    box('Upper front wall',(.16,4.8,3.5),(.08,0,4.65),wall,g,0)
    for s in (-1,1):box('Entry side wall',(.16,1.2,2.9),(.08,s*1.8,1.45),wall,g,0)
    box('Stairwell roof',(4,4.8,.16),(2,0,6.48),wall,g,0)
    for x in (.76,2.04):
        tube('Lower handrail',(x,-1.28,1.0),(x,1.40,2.6),.022,metal,g)
        for i in (0,3,6,9):
            z=(i+1)*.16;y=-1.15+i*.27;tube('Lower rail post',(x,y,z),(x,y,z+.95),.016,metal,g)
    for x in (2.16,3.44):
        tube('Upper handrail',(x,1.40,2.6),(x,-1.28,4.2),.022,metal,g)
        for i in (0,3,6,9):
            z=1.6+(i+1)*.16;y=1.28-i*.27;tube('Upper rail post',(x,y,z),(x,y,z+.95),.016,metal,g)
    box('Upper floor exit leaf',(.85,.065,2.1),(2.8,-2.195,4.25),paint,g,.003)
    for x in (2.345,3.255):box('Upper exit jamb',(.06,.11,2.2),(x,-2.185,4.3),metal,g)
    box('Upper exit header',(.97,.11,.06),(2.8,-2.185,5.42),metal,g)
    tube('Upper exit handle',(3.12,-2.14,4.2),(3.12,-2.05,4.2),.013,metal,g)
    return g

collection=bpy.data.collections.get(COLLECTION)
if collection:
    for o in list(collection.objects):bpy.data.objects.remove(o,do_unlink=True)
else:
    collection=bpy.data.collections.new(COLLECTION);bpy.context.scene.collection.children.link(collection)
paint=material('SchoolPaint',(.14,.25,.255),.08,.43)
metal=material('SchoolMetal',(.48,.51,.52),.72,.31)
seal=material('SchoolSeal',(.065,.075,.077),0,.65)
glass=material('SchoolGlass',(.55,.66,.65),.12,.32)
stone=material('SchoolStairStone',(.60,.59,.55),0,.7)
wall=material('SchoolStairWall',(.72,.72,.66),0,.78)
door('SchoolDoorSolid',1.1,2.2,.06)
door('SchoolDoorGlazed',.88,2.2,.08,True)
door('SchoolDoorOpen',1.3,2.26,.06,False,False)
f=group('SchoolDoorFrameOpen');frame('Open frame',1.4,2.3,.16,f,0,.06)
window();elevator();stairwell()
# Pack all original mesh pieces per model/material to reduce static renderer counts.
for root in [o for o in list(collection.objects) if o.type=='EMPTY']:
    meshes=[o for o in list(collection.objects) if o.parent==root and o.type=='MESH']
    if meshes:
        bpy.ops.object.select_all(action='DESELECT')
        for o in meshes:o.select_set(True)
        bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join()
        merged=bpy.context.object;merged.name=root.name+'Mesh'
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
os.makedirs(ROOT+'/Source~',exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=ROOT+'/Source~/SchoolArchitecture.blend')
bpy.ops.object.select_all(action='DESELECT')
for o in collection.objects:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=ROOT+'/SchoolArchitecture.fbx',use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',use_mesh_modifiers=True,bake_anim=False,add_leaf_bones=False)
result={'blend':ROOT+'/Source~/SchoolArchitecture.blend','fbx':ROOT+'/SchoolArchitecture.fbx','models':[o.name for o in collection.objects if o.type=='EMPTY'],'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in collection.objects if o.type=='MESH')}
