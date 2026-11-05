"""Edit the CC0 Dave Welsh recording with Blender aud and numpy; no synthetic tones.
Source: https://freesound.org/people/Dave%20Welsh/sounds/523317/
The supplied source is the publisher's 48 kHz high-quality MP3 preview, not the 96 kHz master.
"""
import aud, numpy as np, wave, os, json, hashlib
root='E:/projects/T3_LWS/Assets/_02LWS'
source=root+'/Audio/Alarm/Source-ElectricBell.mp3'
sound=aud.Sound(source)
rate=int(sound.specs[0]); samples=sound.data().mean(axis=1).astype(np.float64)
start=56.0; duration=8.0; overlap=.1
raw=samples[int(start*rate):int((start+duration+overlap)*rate)].copy()
raw-=raw.mean()
n=int(rate*overlap)
out=raw[:-n].copy()
t=np.arange(n)/n
out[:n]=raw[-n:]*(1-t)+raw[:n]*t
gain=.5/np.max(np.abs(out)); out*=gain
pcm=np.round(np.clip(out,-1,1)*32767).astype('<i2')
dest=root+'/Audio/Alarm/CorridorAlarm.wav'
with wave.open(dest,'wb') as w:
    w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(pcm.tobytes())
info={'source':'Dave Welsh / School Bell - Fire Bell.wav / Freesound 523317',
      'license':'CC0 1.0','download':'https://cdn.freesound.org/previews/523/523317_1985044-hq.mp3',
      'sourceSha256':hashlib.sha256(open(source,'rb').read()).hexdigest(),
      'sourceExcerptSeconds':[start,start+duration+overlap],
      'sampleRate':rate,'channels':1,'bits':16,'durationSeconds':len(pcm)/rate,
      'crossfadeSeconds':overlap,'peak':float(abs(out).max()),
      'loopBoundaryJump':float(abs(out[-1]-out[0])),
      'sha256':hashlib.sha256(open(dest,'rb').read()).hexdigest(),
      'scope':'Recorded solenoid electric bell; not a recording of the modeled Korean product. Headset listening approval pending.'}
with open(root+'/Audio/Alarm/RecordingInfo.json','w',encoding='utf8') as f:json.dump(info,f,indent=2)
result=info
