Decals would be a new mesh calculated from the vertices of the terrain mesh. 

The area for a decal would be a quad and the new mesh would be calculated by finding all triangles of the terrain mesh that intersect with the quad. These intersecting triangles should then be formed into a new mesh. 

The texture mapping on the new mesh should be as simple as "distance from left edge and top of quad" for each vertex.