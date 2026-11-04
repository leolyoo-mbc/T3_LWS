# Free visual asset correction — 2026-11-04

User reported sausage-shaped fire, one-armed mannequin people and unnatural motion. Replaced the primitive fire with transparent Kenney flame/smoke particle textures and replaced the people with a clothed Quaternius character and Quaternius Standard idle/talking clips. These selected external visual assets are CC0; see ThirdPartyNotices.md and VisualAssetSources.json.

The model's independent IK feet were reparented to lower legs while retaining bind matrices. Humanoid avatars initially imported as valid but had different reference poses and produced raised arms/crossed legs. Sampling bind poses and enforcing Unity's humanoid T-pose before recording HumanDescription corrected retargeting. Public human-motion clips are selected explicitly; importer preview clips are excluded.

Editor Play Mode checks passed: all three people animate, hands remain below heads during conversation, reaction enters Attentive, freezing stops skeletons and all three particle systems, restoring resumes Idle and particles. The normal 14-check end-to-end menu/shout/next/bell/completion/return test passed after replacement. Captured idle and attentive views were inspected for limb distortion and fire shape.

Reaction blends from talking to neutral idle with a gradual head/body look toward the player or bell. Individual phase offsets prevent synchronized motion. This is a restrained notification reaction, not an evacuation simulation.

Quest 2 appearance, comfort and performance are still unverified because the user cannot connect the device. User visual acceptance remains separate from automated checks.
