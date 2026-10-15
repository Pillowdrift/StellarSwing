import math, sys
out = sys.argv[1]
# ---------- body profile (radius vs height t in [0,1]); R=1 is the widest half-width, height = 1.76
prof = [(1.00,0.00),(0.97,0.30),(0.95,0.45),(0.90,0.57),(0.85,0.69),(0.80,0.77),(0.75,0.83),(0.70,0.88),
        (0.65,0.91),(0.60,0.95),(0.50,0.98),(0.40,1.00),(0.30,0.99),(0.22,1.00),(0.16,1.06),(0.10,1.10),(0.05,1.09),(0.015,0.98),(0.0,0.0)]
prof = sorted(prof)
def body_r(t):
    # Catmull-Rom through the profile knots: smooth radius with continuous slope
    ts=[p[0] for p in prof]; rs=[p[1] for p in prof]
    n=len(prof)
    if t<=ts[0]: return rs[0]
    if t>=ts[-1]: return rs[-1]
    i=max(j for j in range(n-1) if ts[j]<=t)
    t0,t1=ts[i],ts[i+1]; u=(t-t0)/(t1-t0) if t1>t0 else 0.0
    p0=rs[max(i-1,0)]; p1=rs[i]; p2=rs[i+1]; p3=rs[min(i+2,n-1)]
    return 0.5*((2*p1)+(-p0+p2)*u+(2*p0-5*p1+4*p2-p3)*u*u+(-p0+3*p1-3*p2+p3)*u*u*u)
H = 1.76
def flipper(t, th):
    # two soft lobes at the base corners, a touch toward the front, blended into the body
    a = 0.34*math.exp(-((t-0.06)/0.085)**2)
    s = 0
    for c in (math.radians(100), math.radians(-100)):
        d = math.atan2(math.sin(th-c), math.cos(th-c))
        s += math.exp(-(d/0.40)**2)
    return a*s
def surf(t, th):
    r = body_r(t) + flipper(t, th)
    x = r*math.sin(th); z = r*math.cos(th)*0.86   # front (+z) slightly flattened
    y = t*H + 0.22*flipper(t,th)                    # tips curl up a touch
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
    t_e = 0.25; th = math.asin(0.25/body_r(t_e))*sx
    x,y,z = surf(t_e, th)
    # eyes: plain black ovals sitting just proud of the surface, like the logo
    ellipsoid('Dark', (x*0.99, y+0.01, z*0.99), 0.072, 0.084, 0.035, 14, 20)
# nose: small dark rounded wedge just below and between the eyes
x,y,z = surf(0.14, 0.0)
ellipsoid('Dark', (0, y, z*0.99+0.01), 0.055, 0.034, 0.035, 10, 16)
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
with open(out,'w') as fh:
    fh.write('mtllib obo_mascot.mtl\n')
    for p in V: fh.write('v %.5f %.5f %.5f\n'%p)
    for uv in VT: fh.write('vt %.4f %.4f\n'%uv)
    for nn in N: fh.write('vn %.4f %.4f %.4f\n'%nn)
    cur=None
    for g,f in sorted(F, key=lambda x: ['Body','Dark'].index(x[0])):
        if g!=cur:
            cur=g; fh.write('o %s\nusemtl Obo%s\n'%(g,g))
        fh.write('f '+' '.join('%d/%d/%d'%(i,i,i) for i in f)+'\n')
print('verts',len(V),'faces',len(F))
