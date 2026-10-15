import math, sys
out = sys.argv[1]
# ---------- body profile (radius vs height t in [0,1]); R=1 is the widest half-width, height = 1.76
prof = [(1.00,0.00),(0.97,0.30),(0.95,0.45),(0.90,0.57),(0.85,0.69),(0.80,0.77),(0.75,0.83),(0.70,0.88),
        (0.65,0.91),(0.60,0.95),(0.50,0.98),(0.40,1.00),(0.30,0.99),(0.22,1.00),(0.16,1.06),(0.10,1.10),(0.05,1.09),(0.015,0.98),(0.0,0.0)]
prof = sorted(prof)
def body_r(t):
    for (t0,r0),(t1,r1) in zip(prof,prof[1:]):
        if t0 <= t <= t1:
            u = (t-t0)/(t1-t0) if t1>t0 else 0
            u = u*u*(3-2*u)  # smoothstep for a softer curve
            return r0+(r1-r0)*u
    return 0.0
H = 1.76
def flipper(t, th):
    # two bumps at the sides near the base, pointing outward
    a = 0.26*math.exp(-((t-0.07)/0.07)**2)
    s = 0
    for c in (math.pi/2, -math.pi/2):
        d = math.atan2(math.sin(th-c), math.cos(th-c))
        s += math.exp(-(d/0.36)**2)
    return a*s
def surf(t, th):
    r = body_r(t) + flipper(t, th)
    x = r*math.sin(th); z = r*math.cos(th)*0.86   # front (+z) slightly flattened
    y = t*H + 0.06*flipper(t,th)*4                 # flipper tips curl up a touch
    return (x, y, z)
V=[]; VT=[]; F=[]   # faces: list of (group, [vertex idx...]) 1-based
def add_v(p, uv=(0,0)):
    V.append(p); VT.append(uv); return len(V)
def revolve(group, rings=72, segs=96, tmin=0.0, tmax=1.0):
    grid=[]
    for i in range(rings+1):
        t = tmin + (tmax-tmin)*i/rings
        tt = 1-(1-t)**1.0
        row=[]
        for j in range(segs+1):
            th = 2*math.pi*j/segs
            p = surf(tt, th) if tt<1.0 else (0, H, 0)
            row.append(add_v(p, (j/segs, tt)))
        grid.append(row)
    for i in range(rings):
        for j in range(segs):
            a,b,c,d = grid[i][j], grid[i][j+1], grid[i+1][j+1], grid[i+1][j]
            if i < rings-1: F.append((group,[a,b,c])); F.append((group,[a,c,d]))
            else: F.append((group,[a,b,d]))
    # base cap (slightly concave)
    centre = add_v((0, -0.03, 0), (0.5,0))
    base = grid[0]
    for j in range(segs):
        F.append((group,[base[j+1], base[j], centre]))
def ellipsoid(group, c, rx, ry, rz, rings=18, segs=28):
    grid=[]
    for i in range(rings+1):
        ph = math.pi*i/rings
        row=[]
        for j in range(segs+1):
            th = 2*math.pi*j/segs
            p = (c[0]+rx*math.sin(ph)*math.sin(th), c[1]+ry*math.cos(ph), c[2]+rz*math.sin(ph)*math.cos(th))
            row.append(add_v(p,(j/segs,1-i/rings)))
        grid.append(row)
    for i in range(rings):
        for j in range(segs):
            a,b,c2,d = grid[i][j], grid[i][j+1], grid[i+1][j+1], grid[i+1][j]
            if i>0: F.append((group,[a,c2,b]))
            if i<rings-1: F.append((group,[a,d,c2]))
revolve('Body')
# eyes: low on the face, embedded in the front surface
for sx in (-1, 1):
    t_e = 0.215; th = math.asin(0.25/body_r(t_e))*sx
    x,y,z = surf(t_e, th)
    # flat oval eye sitting just proud of the surface (a decal with a little depth)
    c = (x*0.985, y+0.01, z*0.985)
    ellipsoid('Eyes', c, 0.095, 0.11, 0.045)
    # pupil: small dark oval on the front of the eye, a touch up and inward
    pc = (c[0]-0.012*sx, c[1]+0.02, c[2]+0.040)
    ellipsoid('Dark', pc, 0.034, 0.042, 0.014, 12, 18)
    # highlight: tiny white speck top-left of the pupil
    ellipsoid('Body', (pc[0]-0.012, pc[1]+0.016, pc[2]+0.012), 0.010, 0.010, 0.006, 6, 10)
# paws: little rounded tips at the base corners
for sx in (-1, 1):
    ellipsoid('Body', (sx*1.08, 0.10, 0.30), 0.20, 0.085, 0.17, 14, 20)
    ellipsoid('Body', (sx*1.22, 0.13, 0.33), 0.09, 0.06, 0.09, 10, 14)
# nose: small dark rounded wedge just below and between the eyes
x,y,z = surf(0.14, 0.0)
ellipsoid('Dark', (0, y, z*0.99+0.01), 0.055, 0.034, 0.035, 10, 16)
# ---------- ink hull: offset copies with reversed winding
def vertex_normals():
    n=[[0.0,0.0,0.0] for _ in V]
    key=[tuple(round(c,4) for c in p) for p in V]
    acc={}
    for g,f in F:
        p0,p1,p2 = V[f[0]-1],V[f[1]-1],V[f[2]-1]
        u=(p1[0]-p0[0],p1[1]-p0[1],p1[2]-p0[2]); w=(p2[0]-p0[0],p2[1]-p0[1],p2[2]-p0[2])
        nx,ny,nz = u[1]*w[2]-u[2]*w[1], u[2]*w[0]-u[0]*w[2], u[0]*w[1]-u[1]*w[0]
        for i in f:
            a=acc.setdefault(key[i-1],[0.0,0.0,0.0]); a[0]+=nx; a[1]+=ny; a[2]+=nz
    n=[acc[k] for k in key]
    res=[]
    for a in n:
        l=math.sqrt(a[0]**2+a[1]**2+a[2]**2) or 1
        res.append((a[0]/l,a[1]/l,a[2]/l))
    return res
N = vertex_normals()
base_faces = list(F); nbase = len(V)
offset = {'Body':0.055,'Eyes':0.014,'Dark':0.0}
remap = {}
for g,f in base_faces:
    d = offset[g]
    if d == 0: continue
    nf=[]
    for i in f:
        if i not in remap:
            p=V[i-1]; n=N[i-1]
            remap[i]=add_v((p[0]+n[0]*d, p[1]+n[1]*d, p[2]+n[2]*d), VT[i-1])
        nf.append(remap[i])
    F.append(('Ink', nf[::-1]))
with open(out,'w') as fh:
    fh.write('mtllib obo_mascot.mtl\n')
    for p in V: fh.write('v %.5f %.5f %.5f\n'%p)
    for uv in VT: fh.write('vt %.4f %.4f\n'%uv)
    NN = vertex_normals()
    for g,f in F:
        pass
    for nn in NN: fh.write('vn %.4f %.4f %.4f\n'%nn)
    cur=None
    for g,f in sorted(F, key=lambda x: ['Body','Eyes','Dark','Ink'].index(x[0])):
        if g!=cur:
            cur=g; fh.write('o %s\nusemtl Obo%s\n'%(g,g))
        fh.write('f '+' '.join('%d/%d/%d'%(i,i,i) for i in f)+'\n')
print('verts',len(V),'faces',len(F))
