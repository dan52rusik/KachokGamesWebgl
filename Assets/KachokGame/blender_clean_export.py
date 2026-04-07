"""
Blender Script: Clean Rigify Rig for Unity Humanoid
=====================================================
Builds proper bone hierarchy from flat DEF- bones.

HOW TO USE:
1. Open your .blend file in Blender
2. Go to Scripting workspace (top tabs)
3. Click "New" to create a new text block
4. Paste this entire script
5. Click "Run Script" (▶)
6. The cleaned FBX will be saved next to your .blend file as 'KachokPlayer_Clean.fbx'
"""

import bpy
import os

# ========================================================
# RIGIFY DEF BONE HIERARCHY (standard metarig structure)
# Format: child_bone -> parent_bone
# After stripping DEF- prefix
# ========================================================
BONE_HIERARCHY = {
    # Spine chain
    "spine":     None,          # ROOT - Hips
    "spine.001": "spine",       # Spine
    "spine.002": "spine.001",   # Chest
    "spine.003": "spine.002",   # Upper Chest
    "spine.004": "spine.003",   # Neck
    "spine.005": "spine.004",   # Neck 2
    "spine.006": "spine.005",   # Head

    # Left leg
    "thigh.L":     "spine",         # Left Upper Leg
    "thigh.L.001": "thigh.L",
    "shin.L":      "thigh.L.001",   # Left Lower Leg
    "shin.L.001":  "shin.L",
    "foot.L":      "shin.L.001",    # Left Foot
    "toe.L":       "foot.L",        # Left Toes

    # Right leg
    "thigh.R":     "spine",         # Right Upper Leg
    "thigh.R.001": "thigh.R",
    "shin.R":      "thigh.R.001",   # Right Lower Leg
    "shin.R.001":  "shin.R",
    "foot.R":      "shin.R.001",    # Right Foot
    "toe.R":       "foot.R",        # Right Toes

    # Left arm
    "shoulder.L":      "spine.003",       # Left Shoulder
    "upper_arm.L":     "shoulder.L",      # Left Upper Arm
    "upper_arm.L.001": "upper_arm.L",
    "forearm.L":       "upper_arm.L.001", # Left Lower Arm
    "forearm.L.001":   "forearm.L",
    "hand.L":          "forearm.L.001",   # Left Hand

    # Right arm
    "shoulder.R":      "spine.003",       # Right Shoulder
    "upper_arm.R":     "shoulder.R",      # Right Upper Arm
    "upper_arm.R.001": "upper_arm.R",
    "forearm.R":       "upper_arm.R.001", # Right Lower Arm
    "forearm.R.001":   "forearm.R",
    "hand.R":          "forearm.R.001",   # Right Hand

    # Extra bones
    "pelvis.L":  "spine",       # Left pelvis
    "pelvis.R":  "spine",       # Right pelvis
    "breast.L":  "spine.003",   # Left breast
    "breast.R":  "spine.003",   # Right breast
}


def clean_and_export():
    print("\n" + "=" * 60)
    print("  RIGIFY → UNITY HUMANOID CLEANUP")
    print("=" * 60)

    # ---- Step 1: Find armature ----
    rig = bpy.data.objects.get("rig")
    if not rig or rig.type != 'ARMATURE':
        for obj in bpy.data.objects:
            if obj.type == 'ARMATURE' and obj.name != 'metarig':
                rig = obj
                break

    if not rig:
        print("[ERROR] No armature found!")
        return

    # Find mesh
    mesh_obj = None
    for obj in bpy.data.objects:
        if obj.type == 'MESH' and not obj.name.startswith("WGT-"):
            mesh_obj = obj
            break

    print(f"[1] Armature: '{rig.name}', Mesh: '{mesh_obj.name if mesh_obj else 'NONE'}'")

    # ---- Step 2: Remove metarig ----
    metarig = bpy.data.objects.get("metarig")
    if metarig:
        bpy.data.objects.remove(metarig, do_unlink=True)
        print("[2] ✓ Removed metarig")
    else:
        print("[2] ~ No metarig found")

    # ---- Step 3: Remove WGT- widget objects ----
    wgt_objects = [obj for obj in bpy.data.objects if obj.name.startswith("WGT-")]
    for obj in wgt_objects:
        bpy.data.objects.remove(obj, do_unlink=True)
    print(f"[3] ✓ Removed {len(wgt_objects)} WGT- widget objects")

    # ---- Step 4: Edit armature bones ----
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode='EDIT')

    armature = rig.data
    edit_bones = armature.edit_bones

    # Identify DEF- bones
    def_bone_names = [b.name for b in edit_bones if b.name.startswith("DEF-")]
    print(f"[4] Found {len(def_bone_names)} DEF- bones: {', '.join(sorted(def_bone_names))}")

    # Remove all non-DEF bones
    non_def = [b for b in edit_bones if not b.name.startswith("DEF-")]
    for b in non_def:
        edit_bones.remove(b)
    print(f"[4] ✓ Removed {len(non_def)} non-DEF bones")

    # ---- Step 5: Rename DEF- bones (strip prefix) ----
    rename_map = {}
    for bone in list(edit_bones):
        if bone.name.startswith("DEF-"):
            old_name = bone.name
            new_name = old_name[4:]  # Strip "DEF-"
            bone.name = new_name
            rename_map[old_name] = new_name

    print(f"[5] ✓ Renamed {len(rename_map)} bones (stripped 'DEF-' prefix)")

    # ---- Step 6: Rebuild hierarchy ----
    # This is the crucial step - set correct parent-child relationships
    hierarchy_applied = 0
    hierarchy_skipped = 0

    for child_name, parent_name in BONE_HIERARCHY.items():
        child_bone = edit_bones.get(child_name)
        if not child_bone:
            print(f"  [!] Bone '{child_name}' not found, skipping")
            hierarchy_skipped += 1
            continue

        if parent_name is None:
            child_bone.parent = None  # Root bone
            hierarchy_applied += 1
        else:
            parent_bone = edit_bones.get(parent_name)
            if parent_bone:
                child_bone.parent = parent_bone
                child_bone.use_connect = False  # Don't force connected
                hierarchy_applied += 1
            else:
                print(f"  [!] Parent '{parent_name}' not found for '{child_name}'")
                hierarchy_skipped += 1

    print(f"[6] ✓ Applied {hierarchy_applied} parent relationships ({hierarchy_skipped} skipped)")

    # ---- Step 7: Print final hierarchy ----
    print("\n[7] Final bone hierarchy:")
    root_bones = [b for b in edit_bones if b.parent is None]

    def print_tree(bone, depth=0):
        indent = "  " * depth
        marker = "●" if depth == 0 else "└─"
        n_children = len(bone.children)
        print(f"  {indent}{marker} {bone.name} ({n_children} children)")
        for child in sorted(bone.children, key=lambda b: b.name):
            print_tree(child, depth + 1)

    for root in sorted(root_bones, key=lambda b: b.name):
        print_tree(root)

    bpy.ops.object.mode_set(mode='OBJECT')

    # ---- Step 8: Fix vertex group names on mesh ----
    if mesh_obj:
        renamed_vg = 0
        for vg in mesh_obj.vertex_groups:
            if vg.name in rename_map:
                vg.name = rename_map[vg.name]
                renamed_vg += 1
        print(f"\n[8] ✓ Updated {renamed_vg} vertex group names")
    else:
        print("\n[8] ~ No mesh found")

    # ---- Step 9: Apply transforms ----
    bpy.ops.object.select_all(action='DESELECT')
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    if mesh_obj:
        bpy.ops.object.select_all(action='DESELECT')
        mesh_obj.select_set(True)
        bpy.context.view_layer.objects.active = mesh_obj
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    print("[9] ✓ Applied transforms")

    # ---- Step 10: Export FBX ----
    blend_path = bpy.data.filepath
    if blend_path:
        export_dir = os.path.dirname(blend_path)
    else:
        export_dir = os.path.expanduser("~/Desktop")

    export_path = os.path.join(export_dir, "KachokPlayer_Clean.fbx")

    bpy.ops.object.select_all(action='DESELECT')
    rig.select_set(True)
    if mesh_obj:
        mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = rig

    bpy.ops.export_scene.fbx(
        filepath=export_path,
        use_selection=True,
        object_types={'ARMATURE', 'MESH'},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        bake_anim=False,
        apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',
        axis_up='Y',
    )

    print(f"\n{'=' * 60}")
    print(f"  ✅ EXPORT COMPLETE!")
    print(f"  File: {export_path}")
    print(f"{'=' * 60}")
    print(f"\nNext steps:")
    print(f"  1. Copy 'KachokPlayer_Clean.fbx' to Unity Assets/KachokGame/")
    print(f"  2. Select it → Rig tab → Humanoid → Apply")
    print(f"  3. Run: Tools → Kachok → Setup Humanoid Rig (Auto)")


# Run
clean_and_export()
