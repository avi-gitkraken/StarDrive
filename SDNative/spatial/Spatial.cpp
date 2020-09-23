#include "Spatial.h"
#include "grid/Grid.h"
#include "qtree/Qtree.h"

namespace spatial
{
    std::shared_ptr<Spatial> Spatial::create(SpatialType type, int universeSize, int cellSize)
    {
        if (type == SpatialType::Grid)
            return std::make_shared<Grid>(universeSize, cellSize);
        if (type == SpatialType::QuadTree)
            return std::make_shared<Qtree>(universeSize, cellSize);
        return {};
    }

    SPATIAL_C_API Spatial* SpatialCreate(SpatialType type, int universeSize, int cellSize)
    {
        if (type == SpatialType::Grid)
            return new Grid{universeSize, cellSize};
        if (type == SpatialType::QuadTree)
            return new Qtree{universeSize, cellSize};
        return nullptr;
    }
    SPATIAL_C_API void SpatialDestroy(Spatial* spatial)
    {
        delete spatial;
    }
    
    SPATIAL_C_API SpatialType SpatialGetType(Spatial* spatial) { return spatial->type(); }
    SPATIAL_C_API int SpatialWorldSize(Spatial* spatial) { return spatial->worldSize(); }
    SPATIAL_C_API int SpatialFullSize(Spatial* spatial)  { return spatial->fullSize(); }
    SPATIAL_C_API int SpatialCount(Spatial* spatial)     { return spatial->count(); }
    SPATIAL_C_API void SpatialClear(Spatial* spatial)    { spatial->clear(); }
    SPATIAL_C_API void SpatialRebuild(Spatial* spatial)  { spatial->rebuild(); }

    SPATIAL_C_API int SpatialInsert(Spatial* spatial, const SpatialObject* o)
    {
        return spatial->insert(*o);
    }
    SPATIAL_C_API void SpatialUpdate(Spatial* spatial, int objectId, int x, int y)
    {
        spatial->update(objectId, x, y);
    }
    SPATIAL_C_API void SpatialRemove(Spatial* spatial, int objectId)
    {
        spatial->remove(objectId);
    }

    SPATIAL_C_API void SpatialCollideAll(Spatial* spatial, float timeStep, void* user, CollisionFunc onCollide)
    {
        spatial->collideAll(timeStep, user, onCollide);
    }
    SPATIAL_C_API int SpatialFindNearby(Spatial* spatial, int* outResults, const SearchOptions* opt)
    {
        return spatial->findNearby(outResults, *opt);
    }
    SPATIAL_C_API void SpatialDebugVisualize(Spatial* spatial, const VisualizerOptions* opt, const VisualizerBridge* vis)
    {
        struct CppToCBridge : Visualizer
        {
            VisualizerBridge vis;
            explicit CppToCBridge(const VisualizerBridge& visualizer) : vis{visualizer} {}
            void drawRect(int x1, int y1, int x2, int y2, Color c) override
            { vis.drawRect(x1, y1, x2, y2, c); }
            void drawCircle(int x, int y, int radius, Color c) override
            { vis.drawCircle(x, y, radius, c); }
            void drawLine(int x1, int y1, int x2, int y2, Color c) override
            { vis.drawLine(x1, y1, x2, y2, c); }
            void drawText(int x, int y, int size, const char* text, Color c) override
            { vis.drawText(x, y, size, text, c); }
        };

        CppToCBridge bridge { *vis };
        spatial->debugVisualize(*opt, bridge);
    }
}
